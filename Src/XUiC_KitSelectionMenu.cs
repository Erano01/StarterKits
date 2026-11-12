using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using StarterKits.Kits;

/// <summary>
/// Starter kit seçim menüsü controller'ı.
/// XML'deki event_onclick="eventChooseKit" ile çalışır.
/// </summary>
namespace StarterKits
{
    public class XUiC_KitSelectionMenu : XUiController
    {
        // Enable server-side one-time selection lock.
        private const bool EnableOneTimeSelectionLock = true;

        // Layout slot counts. Must match Config/XUi_InGame/windows.xml (starterKitWindow).
        private const int KitCardCount = 8;
        private const int MaxStatRows = 16;
        private const int ItemSlotCount = 10;

        private const string FloorEnabledVar = "skFloorEnabled";
        private const string FloorVarPrefix = "skFloor_";

        private static readonly string[] CandidateTextPropertyNames = { "Text", "Caption" };
        private static readonly string[] CandidateTextKeyPropertyNames = { "TextKey", "CaptionKey" };
        private static readonly Color NoQualityBorder = new Color32(90, 90, 90, 255);

        /// <summary>One clickable kit card in the left list.</summary>
        private sealed class KitCard
        {
            public XUiController Root;
            public XUiController Hover;
            public XUiController Selected;
            public XUiController Locked;
            public XUiV_Sprite Image;
            public XUiV_Texture ImageFile;
            public XUiV_Label Name;
            public XUiV_Label Title;
            public string KitId;
        }

        private sealed class ItemSlot
        {
            public XUiController Root;
            public XUiV_Sprite Border;
            public XUiV_Sprite Icon;
            public XUiV_Label Quality;
            public XUiV_Label Count;
            public XUiV_Label Name;
        }

        // ---- data ----
        // Kit definitions live in Kits/defaults.xml + Kits/kits.xml (main menu Kit Editor), or come from
        // the server in multiplayer. They are re-read every time this window opens.
        private List<KitDefinition> kits = new List<KitDefinition>();
        private string kitLoadMessage;
        private string selectedKitName;
        private string playerLockedKitName;
        private int kitScroll;
        private int statPage;
        private int itemPage;

        // ---- views ----
        private readonly KitCard[] cards = new KitCard[KitCardCount];
        private readonly ItemSlot[] itemSlots = new ItemSlot[ItemSlotCount];
        private readonly XUiController[] statIconControllers = new XUiController[MaxStatRows];
        private readonly XUiController[] statLabelControllers = new XUiController[MaxStatRows];
        private XUiController previewSprite;
        private XUiController previewFileTexture;
        private XUiController previewLabel;
        private XUiController selectedKitNameLabel;
        private XUiController kitTitleLabel;
        private XUiController attributesLabel;
        private XUiController overviewHintLabel;
        private XUiController lockedKitLabel;
        private XUiController kitCountLabel;
        private XUiController statsPageLabel;
        private XUiController itemsPageLabel;
        private XUiController noItemsLabel;
        private XUiV_Sprite listTrack;
        private XUiV_Sprite listThumb;
        private Vector2i thumbBasePosition;
        private int trackHeight;
        private XUiC_SimpleButton confirmButton;

        static XUiC_KitSelectionMenu()
        {
            // PlayerJoinedGame event handler'ını kaydetmek için sınıfı "uyandır".
            PlayerJoinedGameHandler.Init();
        }

        // =====================================================================
        // Lifecycle
        // =====================================================================

        public override void Init()
        {
            base.Init();

            for (int i = 0; i < KitCardCount; i++)
            {
                this.cards[i] = this.BindCard(i);
            }

            for (int i = 0; i < ItemSlotCount; i++)
            {
                this.itemSlots[i] = new ItemSlot
                {
                    Root = this.GetChildById("itemSlot" + i),
                    Border = this.ViewOf<XUiV_Sprite>("itemSlotQ" + i),
                    Icon = this.ViewOf<XUiV_Sprite>("itemSlotIcon" + i),
                    Quality = this.ViewOf<XUiV_Label>("itemSlotQLabel" + i),
                    Count = this.ViewOf<XUiV_Label>("itemSlotCount" + i),
                    Name = this.ViewOf<XUiV_Label>("itemSlotName" + i)
                };
            }

            for (int i = 0; i < MaxStatRows; i++)
            {
                this.statIconControllers[i] = this.GetChildById("statIcon" + i);
                this.statLabelControllers[i] = this.GetChildById("statLabel" + i);
            }

            this.previewSprite = this.GetChildById("texKitPreview");
            this.previewFileTexture = this.GetChildById("texKitPreviewFile");
            this.previewLabel = this.GetChildById("lblKitImagePlaceholder");
            this.selectedKitNameLabel = this.GetChildById("lblSelectedKitName");
            this.kitTitleLabel = this.GetChildById("lblKitTitle");
            this.attributesLabel = this.GetChildById("lblKitAttributes");
            this.overviewHintLabel = this.GetChildById("lblOverviewHint");
            this.lockedKitLabel = this.GetChildById("lblLockedKit");
            this.kitCountLabel = this.GetChildById("lblKitCount");
            this.statsPageLabel = this.GetChildById("lblStatsPage");
            this.itemsPageLabel = this.GetChildById("lblItemsPage");
            this.noItemsLabel = this.GetChildById("lblNoItems");

            this.listTrack = this.ViewOf<XUiV_Sprite>("kitListTrack");
            this.listThumb = this.ViewOf<XUiV_Sprite>("kitListThumb");
            if (this.listThumb != null && this.listTrack != null)
            {
                this.thumbBasePosition = this.listThumb.Position;
                this.trackHeight = this.listTrack.Size.y;
            }

            // Mouse wheel: kit list scrolls one card, stats/items flip a page.
            this.OnScrollOf("kitListArea", delta => this.ScrollKits(delta > 0 ? -1 : 1));
            this.OnScrollOf("statsArea", delta => this.FlipStats(delta > 0 ? -1 : 1));
            this.OnScrollOf("itemsArea", delta => this.FlipItems(delta > 0 ? -1 : 1));

            var confirmController = this.GetChildById("btnConfirmKit");
            this.confirmButton = confirmController as XUiC_SimpleButton ?? confirmController?.GetChildByType<XUiC_SimpleButton>();
            if (this.confirmButton != null)
            {
                this.confirmButton.OnPressed += this.OnConfirmButtonPressed;
            }
            else
            {
                Log.Warning("[StarterKits] Confirm button could not be resolved; confirm will not trigger rewards.");
            }

            // Multiplayer: kits/pictures from the server may arrive after this window opened.
            Network.KitSync.Changed += this.OnServerKitsChanged;

            this.ReloadKits();
            this.LoadPlayerLockedKit();
            this.RefreshUI();
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.ReloadKits();
            this.LoadPlayerLockedKit();
            this.RefreshUI();
        }

        /// <summary>Kept for XML compatibility (event_onclick="eventChooseKit").</summary>
        public void eventChooseKit(XUiController sender, string _onClick)
        {
            if (!string.IsNullOrEmpty(_onClick))
            {
                this.SelectKit(_onClick);
            }
        }

        public void eventConfirmKit(XUiController sender, string _onClick)
        {
            this.OnConfirmButtonPressed(sender, -1);
        }

        private KitCard BindCard(int index)
        {
            var card = new KitCard
            {
                Root = this.GetChildById("kitCard" + index),
                Hover = this.GetChildById("kitCardHover" + index),
                Selected = this.GetChildById("kitCardSel" + index),
                Locked = this.GetChildById("kitCardLock" + index),
                Image = this.ViewOf<XUiV_Sprite>("kitCardImg" + index),
                ImageFile = this.ViewOf<XUiV_Texture>("kitCardTex" + index),
                Name = this.ViewOf<XUiV_Label>("kitCardName" + index),
                Title = this.ViewOf<XUiV_Label>("kitCardTitle" + index)
            };

            if (card.Root == null)
            {
                Log.Warning($"[StarterKits] Kit card {index} not found in the window XML.");
                return card;
            }

            card.Root.OnPress += (_sender, _mouseButton) =>
            {
                if (!string.IsNullOrEmpty(card.KitId))
                {
                    this.SelectKit(card.KitId);
                }
            };
            card.Root.OnHover += (_sender, _isOver) => this.SetVisible(card.Hover, _isOver && card.KitId != null);
            card.Root.OnScroll += (_sender, _delta) => this.ScrollKits(_delta > 0 ? -1 : 1);
            return card;
        }

        private T ViewOf<T>(string id) where T : XUiView
        {
            return this.GetChildById(id)?.ViewComponent as T;
        }

        private void OnScrollOf(string id, Action<float> onScroll)
        {
            XUiController controller = this.GetChildById(id);
            if (controller != null)
            {
                controller.OnScroll += (_sender, _delta) => onScroll(_delta);
            }
        }

        // =====================================================================
        // Data
        // =====================================================================

        /// <summary>Re-reads Kits/kits.xml (or defaults / server kits) so edits show up without a restart.</summary>
        private void ReloadKits()
        {
            this.kits = KitRepository.LoadKits(out string message);
            this.kitLoadMessage = message;
            Log.Out($"[StarterKits] Kit selection: {message}");
            this.SetText(this.kitCountLabel, this.kits.Count == 1 ? "1 kit available" : $"{this.kits.Count} kits available");
            this.RefreshKitList();
        }

        private void OnServerKitsChanged()
        {
            if (this.xui == null)
            {
                return;
            }

            this.ReloadKits();

            // Keep what the player is looking at, unless the server no longer has that kit.
            if (!string.IsNullOrEmpty(this.selectedKitName) && this.FindKit(this.selectedKitName) == null)
            {
                this.selectedKitName = string.IsNullOrEmpty(this.playerLockedKitName) ? null : this.playerLockedKitName;
            }

            this.ShowKit(this.selectedKitName, false);
        }

        private KitDefinition FindKit(string kitId)
        {
            if (string.IsNullOrEmpty(kitId))
            {
                return null;
            }

            for (int i = 0; i < this.kits.Count; i++)
            {
                if (string.Equals(this.kits[i].Id, kitId, StringComparison.OrdinalIgnoreCase))
                {
                    return this.kits[i];
                }
            }

            return null;
        }

        private string DisplayNameOf(string kitId)
        {
            KitDefinition kit = this.FindKit(kitId);
            return kit != null && !string.IsNullOrEmpty(kit.Name) ? kit.Name : kitId;
        }

        private void LoadPlayerLockedKit()
        {
            var player = this.xui?.playerUI?.entityPlayer;
            if (player == null)
            {
                this.playerLockedKitName = null;
                return;
            }

            if (player.Buffs != null && player.Buffs.HasCustomVar("starterKitSelected"))
            {
                try
                {
                    float kitValue = player.Buffs.GetCustomVar("starterKitSelected");
                    if (kitValue > 0)
                    {
                        this.playerLockedKitName = StarterKitSelectionStore.GetSelected(player);
                        return;
                    }
                }
                catch
                {
                    // Ignore and try store below.
                }
            }

            this.playerLockedKitName = StarterKitSelectionStore.GetSelected(player);
        }

        // =====================================================================
        // Selection
        // =====================================================================

        private bool IsLocked => !string.IsNullOrEmpty(this.playerLockedKitName);

        private void RefreshUI()
        {
            // A locked player opens on their own kit; everyone else starts with nothing selected.
            this.selectedKitName = this.IsLocked ? this.playerLockedKitName : null;
            this.ShowKit(this.selectedKitName, true);
        }

        private void SelectKit(string kitId)
        {
            this.selectedKitName = kitId;
            this.ShowKit(kitId, false);
        }

        private void ShowKit(string kitId, bool scrollIntoView)
        {
            if (scrollIntoView)
            {
                int index = this.kits.FindIndex(k => string.Equals(k.Id, kitId, StringComparison.OrdinalIgnoreCase));
                if (index >= 0 && (index < this.kitScroll || index >= this.kitScroll + KitCardCount))
                {
                    this.kitScroll = index - KitCardCount / 2;
                }

                this.RefreshKitList();
            }

            this.statPage = 0;
            this.itemPage = 0;
            this.RefreshCardStates();
            this.UpdateOverview(kitId);
            this.UpdateFooter();
        }

        private void UpdateFooter()
        {
            if (this.IsLocked)
            {
                this.SetText(this.lockedKitLabel, $"Your kit: {this.DisplayNameOf(this.playerLockedKitName)}  -  locked for this save");
            }
            else
            {
                this.SetText(this.lockedKitLabel, string.Empty);
            }

            if (this.confirmButton != null)
            {
                this.confirmButton.Enabled = !this.IsLocked && this.FindKit(this.selectedKitName) != null;
            }
        }

        // =====================================================================
        // Kit list
        // =====================================================================

        private void ScrollKits(int delta)
        {
            int before = this.kitScroll;
            this.kitScroll += delta;
            this.RefreshKitList();
            if (before != this.kitScroll)
            {
                this.RefreshCardStates();
            }
        }

        private void RefreshKitList()
        {
            int maxScroll = Math.Max(0, this.kits.Count - KitCardCount);
            this.kitScroll = Math.Max(0, Math.Min(this.kitScroll, maxScroll));

            for (int i = 0; i < KitCardCount; i++)
            {
                KitCard card = this.cards[i];
                if (card?.Root == null)
                {
                    continue;
                }

                int index = this.kitScroll + i;
                bool used = index < this.kits.Count;
                card.KitId = used ? this.kits[index].Id : null;
                this.SetVisible(card.Root, used);
                if (!used)
                {
                    continue;
                }

                KitDefinition kit = this.kits[index];
                if (card.Name != null)
                {
                    card.Name.Text = string.IsNullOrEmpty(kit.Name) ? kit.Id : kit.Name;
                }

                if (card.Title != null)
                {
                    card.Title.Text = kit.Title ?? string.Empty;
                }

                this.ApplyPortrait(kit, card.Image, card.ImageFile);
            }

            this.UpdateScrollbar(maxScroll);
            this.RefreshCardStates();
        }

        private void RefreshCardStates()
        {
            for (int i = 0; i < KitCardCount; i++)
            {
                KitCard card = this.cards[i];
                if (card?.Root == null)
                {
                    continue;
                }

                bool selected = card.KitId != null && string.Equals(card.KitId, this.selectedKitName, StringComparison.OrdinalIgnoreCase);
                bool locked = card.KitId != null && string.Equals(card.KitId, this.playerLockedKitName, StringComparison.OrdinalIgnoreCase);
                this.SetVisible(card.Selected, selected);
                this.SetVisible(card.Locked, locked);
            }
        }

        private void UpdateScrollbar(int maxScroll)
        {
            if (this.listThumb == null || this.listTrack == null || this.trackHeight <= 0)
            {
                return;
            }

            bool show = maxScroll > 0;
            this.listTrack.IsVisible = show;
            this.listThumb.IsVisible = show;
            if (!show)
            {
                return;
            }

            int thumbHeight = Math.Max(40, this.trackHeight * KitCardCount / this.kits.Count);
            int travel = this.trackHeight - thumbHeight;
            int offset = maxScroll > 0 ? travel * this.kitScroll / maxScroll : 0;
            this.listThumb.Size = new Vector2i(this.listThumb.Size.x, thumbHeight);
            this.listThumb.Position = new Vector2i(this.thumbBasePosition.x, this.thumbBasePosition.y - offset);
        }

        /// <summary>Shows a kit picture: atlas sprite, or an imported file texture (image_atlas="File").</summary>
        private void ApplyPortrait(KitDefinition kit, XUiV_Sprite sprite, XUiV_Texture texture)
        {
            bool hasImage = kit != null && !string.IsNullOrEmpty(kit.Image);
            bool isFile = hasImage && kit.ImageAtlas == KitAtlases.File;

            if (sprite != null)
            {
                if (hasImage && !isFile)
                {
                    sprite.UIAtlas = string.IsNullOrEmpty(kit.ImageAtlas) ? KitAtlases.ItemIcons : kit.ImageAtlas;
                    sprite.SpriteName = kit.Image;
                }

                sprite.IsVisible = hasImage && !isFile;
            }

            if (texture != null)
            {
                Texture2D portrait = isFile ? KitImageStore.GetPortrait(kit.Image) : null;
                texture.Texture = portrait;
                texture.IsVisible = portrait != null;
                texture.SetDirty();
            }
        }

        private void OnConfirmButtonPressed(XUiController sender, int mouseButton)
        {
            Log.Out($"[StarterKits] Confirm pressed. mouseButton={mouseButton}, selectedKit={this.selectedKitName ?? "<null>"}, playerLocked={this.playerLockedKitName ?? "<none>"}");

            if (string.IsNullOrEmpty(this.selectedKitName))
            {
                return;
            }

            var player = this.xui?.playerUI?.entityPlayer;
            if (player == null)
            {
                Log.Warning("[StarterKits] Confirm ignored: no local entityPlayer.");
                return;
            }

            // If player already has a locked kit, prevent re-selection.
            if (!string.IsNullOrEmpty(this.playerLockedKitName))
            {
                Log.Warning($"[StarterKits] Confirm ignored: player already has kit '{this.playerLockedKitName}' locked.");
                return;
            }

            // Lock the kit server-side.
            if (EnableOneTimeSelectionLock)
            {
                this.TrySetStarterKitSelectedVar(player, this.selectedKitName);
                StarterKitSelectionStore.MarkSelected(player, this.selectedKitName);
                this.playerLockedKitName = this.selectedKitName;
            }

            this.ApplySelectedKitRewards(player, this.selectedKitName);

            Log.Out($"[StarterKits] Confirm button pressed for kit '{this.selectedKitName}'. Player selection locked.");

            // Auto-close the selection window after confirming.
            try
            {
                if (this.xui?.playerUI?.windowManager != null)
                {
                    this.xui.playerUI.windowManager.Close("starterKitGroup");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] Could not auto-close kit window: {ex.Message}");
            }
        }

        private void ApplySelectedKitRewards(EntityPlayer player, string kitId)
        {
            if (player == null || string.IsNullOrEmpty(kitId))
            {
                return;
            }

            KitDefinition kit = this.FindKit(kitId);
            if (kit == null)
            {
                Log.Warning($"[StarterKits] No kit definition found for '{kitId}'.");
                return;
            }

            this.ApplyKitFloors(player, kit);
            this.ApplyKitItems(player, kit);
        }

        private void ApplyKitFloors(EntityPlayer player, KitDefinition kit)
        {
            if (player?.Buffs == null)
            {
                return;
            }

            // Clear floors of every known kit first, so only the chosen kit's floors remain.
            this.ClearAllFloorVars(player);

            int applied = 0;
            try
            {
                this.SetBuffCustomVar(player, FloorEnabledVar, 1f);

                foreach (KitProgression progression in kit.Progressions)
                {
                    if (string.IsNullOrEmpty(progression.Name) || progression.Level <= 0)
                    {
                        continue;
                    }

                    this.SetBuffCustomVar(player, FloorVarPrefix + progression.Name, progression.Level);
                    applied++;
                }

                foreach (KitCvar cvar in kit.Cvars)
                {
                    if (!string.IsNullOrEmpty(cvar.Name))
                    {
                        this.SetBuffCustomVar(player, cvar.Name, cvar.Value);
                    }
                }

                Log.Out($"[StarterKits] Applied kit floors for '{kit.Id}': {applied} progression floors.");
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] Failed to apply kit floors for '{kit.Id}': {ex.Message}");
            }
        }

        private void ClearAllFloorVars(EntityPlayer player)
        {
            if (player?.Buffs == null)
            {
                return;
            }

            this.SetBuffCustomVar(player, FloorEnabledVar, 0f);

            // User kits plus defaults, so floors of a kit deleted in the editor are still cleared.
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KitDefinition kit in this.kits)
            {
                foreach (KitProgression progression in kit.Progressions)
                {
                    names.Add(progression.Name);
                }
            }

            foreach (KitDefinition kit in KitRepository.LoadDefaults(out string _))
            {
                foreach (KitProgression progression in kit.Progressions)
                {
                    names.Add(progression.Name);
                }
            }

            foreach (string name in names)
            {
                if (!string.IsNullOrEmpty(name) && player.Buffs.HasCustomVar(FloorVarPrefix + name))
                {
                    this.SetBuffCustomVar(player, FloorVarPrefix + name, 0f);
                }
            }
        }

        private void SetBuffCustomVar(EntityPlayer player, string varName, float value)
        {
            if (player?.Buffs == null || string.IsNullOrEmpty(varName))
            {
                return;
            }

            try
            {
                player.Buffs.SetCustomVar(varName, value, false);
            }
            catch
            {
                try
                {
                    if (player.Buffs.HasCustomVar(varName))
                    {
                        player.Buffs.RemoveCustomVar(varName);
                    }
                }
                catch
                {
                    // Ignore and try AddCustomVar fallback.
                }

                try
                {
                    player.Buffs.AddCustomVar(varName, value);
                }
                catch
                {
                    // Final fallback intentionally ignored.
                }
            }
        }

        private void ApplyKitItems(EntityPlayer player, KitDefinition kit)
        {
            if (player == null || kit.Items.Count == 0)
            {
                return;
            }

            var overflow = new List<ItemStack>();
            int delivered = 0;

            foreach (KitItem reward in kit.Items)
            {
                if (string.IsNullOrEmpty(reward.Name) || reward.Count <= 0)
                {
                    continue;
                }

                ItemClass itemClass = ItemClass.GetItemClass(reward.Name, true);
                if (itemClass == null)
                {
                    Log.Warning($"[StarterKits] Item reward skipped for '{kit.Id}': unknown item '{reward.Name}'.");
                    continue;
                }

                // V3.4: new ItemValue(id) rolls a random quality 1-6 and sizes mod slots for it.
                // Create the item at the intended quality up front so quality and mod slots match.
                int quality = reward.Quality > 0 ? reward.Quality : 1;
                ItemValue itemValue = itemClass.HasQuality
                    ? new ItemValue(itemClass.Id, quality, quality)
                    : new ItemValue(itemClass.Id);

                // Respect the stack size; large counts become several stacks.
                int remaining = reward.Count;
                int stackSize = Math.Max(1, itemClass.Stacknumber.Value);
                while (remaining > 0)
                {
                    int count = Math.Min(remaining, stackSize);
                    remaining -= count;
                    var stack = new ItemStack(itemValue.Clone(), count);
                    if (this.TryAddItemToInventory(player, stack))
                    {
                        delivered += count;
                    }
                    else
                    {
                        overflow.Add(stack);
                    }
                }

                Log.Out($"[StarterKits] Item '{reward.Name}' (qty={reward.Count}, quality={quality}) granted for kit '{kit.Id}'.");
            }

            if (overflow.Count > 0)
            {
                Vector3 dropPos = player.position;
                dropPos.y += 0.25f;

                try
                {
                    GameManager.Instance.DropContentInLootContainerServer(
                        player.entityId,
                        "EntityLootContainerRegular",
                        dropPos,
                        overflow.ToArray(),
                        false,
                        null);

                    Log.Out($"[StarterKits] Kit '{kit.Id}' item overflow dropped as loot bag(s): {overflow.Count} stack(s).");
                }
                catch (Exception ex)
                {
                    Log.Warning($"[StarterKits] Failed to drop overflow loot bag for '{kit.Id}': {ex.Message}");
                }
            }

            Log.Out($"[StarterKits] Kit '{kit.Id}' completed: {delivered} items to inventory, {overflow.Count} stacks to loot bag.");
        }

        private bool TryAddItemToInventory(EntityPlayer player, ItemStack stack)
        {
            if (player?.inventory == null || stack == null || stack.IsEmpty())
            {
                return false;
            }

            try
            {
                int slot;
                if (player.inventory.AddItem(stack, out slot))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] AddItem failed: {ex.Message}");
            }

            return false;
        }

        // =====================================================================
        // Overview (right side)
        // =====================================================================

        private void UpdateOverview(string kitId)
        {
            KitDefinition kit = this.FindKit(kitId);
            if (kit == null)
            {
                this.SetText(this.selectedKitNameLabel, this.kits.Count == 0 ? "No kits yet" : "No Kit Selected");
                this.SetText(this.kitTitleLabel, string.Empty);
                this.SetText(this.attributesLabel, this.kits.Count == 0 && !string.IsNullOrEmpty(this.kitLoadMessage)
                    ? this.kitLoadMessage
                    : "Pick a starter kit on the left to see what it gives you.");
                this.SetText(this.overviewHintLabel, string.Empty);
                this.SetVisible(this.previewLabel, true);
                this.ApplyPortrait(null, this.previewSprite?.ViewComponent as XUiV_Sprite, this.previewFileTexture?.ViewComponent as XUiV_Texture);
                this.ApplyStatRows(null);
                this.ApplyItems(null);
                return;
            }

            this.SetText(this.selectedKitNameLabel, string.IsNullOrEmpty(kit.Name) ? kit.Id : kit.Name);
            this.SetText(this.kitTitleLabel, kit.Title ?? string.Empty);
            this.SetText(this.attributesLabel, kit.Description ?? string.Empty);
            this.SetVisible(this.previewLabel, string.IsNullOrEmpty(kit.Image));
            this.ApplyPortrait(kit, this.previewSprite?.ViewComponent as XUiV_Sprite, this.previewFileTexture?.ViewComponent as XUiV_Texture);

            string hint;
            if (this.IsLocked)
            {
                hint = string.Equals(kit.Id, this.playerLockedKitName, StringComparison.OrdinalIgnoreCase)
                    ? "This is your kit."
                    : "Preview only - you already chose a kit for this save.";
            }
            else
            {
                hint = "Your choice is final for this save.";
            }

            this.SetText(this.overviewHintLabel, hint);
            this.ApplyStatRows(this.DisplayStats(kit));
            this.ApplyItems(kit);
        }

        /// <summary>
        /// Stat lines for the "Perks &amp; Skills" box. Item lines are left out because items have
        /// their own row of slots below.
        /// </summary>
        private List<KitStat> DisplayStats(KitDefinition kit)
        {
            List<KitStat> source = kit.Stats.Count > 0 ? kit.Stats : KitStatBuilder.Build(kit);
            var itemIcons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KitItem item in kit.Items)
            {
                itemIcons.Add(item.Name);
                ItemClass itemClass = ItemClass.GetItemClass(item.Name, true);
                if (itemClass != null)
                {
                    itemIcons.Add(itemClass.GetIconName());
                }
            }

            var result = new List<KitStat>();
            foreach (KitStat stat in source)
            {
                bool isItemLine = stat.Atlas == KitAtlases.ItemIcons && !string.IsNullOrEmpty(stat.Icon) && itemIcons.Contains(stat.Icon);
                if (!isItemLine)
                {
                    result.Add(stat);
                }
            }

            return result;
        }

        private List<KitStat> currentStats;

        private void FlipStats(int direction)
        {
            if (this.currentStats == null)
            {
                return;
            }

            int pages = Math.Max(1, (this.currentStats.Count + MaxStatRows - 1) / MaxStatRows);
            int page = Math.Max(0, Math.Min(this.statPage + direction, pages - 1));
            if (page != this.statPage)
            {
                this.statPage = page;
                this.ApplyStatRows(this.currentStats);
            }
        }

        private static Color ParseIconColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
            {
                return Color.white;
            }

            if (ColorUtility.TryParseHtmlString("#" + hex, out Color color))
            {
                return color;
            }

            return Color.white;
        }

        private void ApplyStatRows(List<KitStat> entries)
        {
            this.currentStats = entries;
            int count = entries?.Count ?? 0;
            int pages = Math.Max(1, (count + MaxStatRows - 1) / MaxStatRows);
            this.statPage = Math.Max(0, Math.Min(this.statPage, pages - 1));
            int first = this.statPage * MaxStatRows;

            if (count == 0)
            {
                this.SetText(this.statsPageLabel, entries == null ? string.Empty : "No perks or skills");
            }
            else if (pages > 1)
            {
                this.SetText(this.statsPageLabel, $"{first + 1}-{Math.Min(first + MaxStatRows, count)} of {count}   (scroll for more)");
            }
            else
            {
                this.SetText(this.statsPageLabel, string.Empty);
            }

            for (int i = 0; i < MaxStatRows; i++)
            {
                XUiController labelController = this.statLabelControllers[i];
                XUiController iconController = this.statIconControllers[i];
                int index = first + i;

                if (index >= count)
                {
                    this.SetText(labelController, string.Empty);
                    this.SetVisible(iconController, false);
                    continue;
                }

                KitStat entry = entries[index];
                this.SetText(labelController, entry.Text);

                if (!string.IsNullOrEmpty(entry.Icon) && iconController?.ViewComponent is XUiV_Sprite spriteView)
                {
                    spriteView.UIAtlas = string.IsNullOrEmpty(entry.Atlas) ? KitAtlases.Ui : entry.Atlas;
                    spriteView.SpriteName = entry.Icon;
                    spriteView.Color = ParseIconColor(entry.Color);
                    this.SetVisible(iconController, true);
                }
                else
                {
                    this.SetVisible(iconController, false);
                }
            }
        }

        private KitDefinition currentItemsKit;

        private void FlipItems(int direction)
        {
            if (this.currentItemsKit == null)
            {
                return;
            }

            int pages = Math.Max(1, (this.currentItemsKit.Items.Count + ItemSlotCount - 1) / ItemSlotCount);
            int page = Math.Max(0, Math.Min(this.itemPage + direction, pages - 1));
            if (page != this.itemPage)
            {
                this.itemPage = page;
                this.ApplyItems(this.currentItemsKit);
            }
        }

        /// <summary>Item rewards as inventory-style slots: icon, count and a quality-coloured border.</summary>
        private void ApplyItems(KitDefinition kit)
        {
            this.currentItemsKit = kit;
            int count = kit?.Items.Count ?? 0;
            int pages = Math.Max(1, (count + ItemSlotCount - 1) / ItemSlotCount);
            this.itemPage = Math.Max(0, Math.Min(this.itemPage, pages - 1));
            int first = this.itemPage * ItemSlotCount;

            this.SetVisible(this.noItemsLabel, kit != null && count == 0);
            this.SetText(this.itemsPageLabel, pages > 1
                ? $"{first + 1}-{Math.Min(first + ItemSlotCount, count)} of {count}   (scroll for more)"
                : string.Empty);

            for (int i = 0; i < ItemSlotCount; i++)
            {
                ItemSlot slot = this.itemSlots[i];
                if (slot?.Root == null)
                {
                    continue;
                }

                int index = first + i;
                bool used = index < count;
                this.SetVisible(slot.Root, used);
                if (!used)
                {
                    continue;
                }

                KitItem item = kit.Items[index];
                ItemClass itemClass = ItemClass.GetItemClass(item.Name, true);

                if (slot.Icon != null)
                {
                    slot.Icon.SpriteName = itemClass != null ? itemClass.GetIconName() : item.Name;
                    slot.Icon.Color = itemClass != null ? itemClass.GetIconTint() : Color.white;
                }

                if (slot.Name != null)
                {
                    slot.Name.Text = itemClass != null ? itemClass.GetLocalizedItemName() : item.Name;
                }

                if (slot.Count != null)
                {
                    slot.Count.Text = item.Count > 1 ? item.Count.ToString() : string.Empty;
                }

                // Same rule as the reward code: quality items without a set quality are granted at 1.
                bool hasQuality = itemClass != null && itemClass.HasQuality;
                int quality = hasQuality ? Math.Max(1, item.Quality) : 0;
                Color qualityColor = hasQuality ? QualityColor(quality) : NoQualityBorder;

                if (slot.Border != null)
                {
                    slot.Border.Color = qualityColor;
                }

                if (slot.Quality != null)
                {
                    slot.Quality.Text = hasQuality ? "Q" + quality : string.Empty;
                    slot.Quality.Color = qualityColor;
                }
            }
        }

        private static Color QualityColor(int quality)
        {
            try
            {
                return QualityInfo.GetQualityColor(quality);
            }
            catch
            {
                return Color.white;
            }
        }

        private void SetVisible(XUiController controller, bool visible)
        {
            if (controller?.ViewComponent == null)
            {
                return;
            }

            controller.ViewComponent.IsVisible = visible;
            controller.ViewComponent.SetDirty();
        }

        private void SetText(XUiController controller, string value)
        {
            if (controller == null)
            {
                return;
            }

            if (this.TryApplyText(controller, value))
            {
                return;
            }

            for (int i = 0; i < controller.Children.Count; i++)
            {
                if (this.TryApplyText(controller.Children[i], value))
                {
                    return;
                }
            }
        }

        private bool TryApplyText(XUiController controller, string value)
        {
            if (controller?.ViewComponent == null)
            {
                return false;
            }

            bool changed = false;
            Type viewType = controller.ViewComponent.GetType();

            for (int i = 0; i < CandidateTextKeyPropertyNames.Length; i++)
            {
                changed |= this.TrySetMember(viewType, controller.ViewComponent, CandidateTextKeyPropertyNames[i], string.Empty);
            }

            for (int i = 0; i < CandidateTextPropertyNames.Length; i++)
            {
                changed |= this.TrySetMember(viewType, controller.ViewComponent, CandidateTextPropertyNames[i], value);
            }

            controller.ViewComponent.SetDirty();
            return changed;
        }

        private bool TrySetMember(Type targetType, object target, string memberName, string value)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            PropertyInfo property = targetType.GetProperty(memberName, Flags);
            if (property != null && property.CanWrite && property.PropertyType == typeof(string))
            {
                property.SetValue(target, value, null);
                return true;
            }

            FieldInfo field = targetType.GetField(memberName, Flags);
            if (field != null && field.FieldType == typeof(string))
            {
                field.SetValue(target, value);
                return true;
            }

            return false;
        }

        private void TrySetStarterKitSelectedVar(EntityPlayer player, string kitName)
        {
            if (player?.Buffs == null)
            {
                return;
            }

            try
            {
                player.Buffs.SetCustomVar("starterKitSelected", 1f, false);
                Log.Out($"[StarterKits] SetCustomVar applied for kit '{kitName}'.");
            }
            catch
            {
                try
                {
                    player.Buffs.AddCustomVar("starterKitSelected", 1f);
                    Log.Out($"[StarterKits] AddCustomVar fallback applied for kit '{kitName}'.");
                }
                catch (Exception ex)
                {
                    Log.Warning($"[StarterKits] Could not set custom vars for kit '{kitName}': {ex.Message}");
                }
            }
        }

        public static void TryOpenForPlayer()
        {
            var world = GameManager.Instance.World;
            if (world == null)
            {
                return;
            }

            var player = world.GetPrimaryPlayer();
            if (player == null)
            {
                return;
            }

            if (EnableOneTimeSelectionLock && player.Buffs.HasCustomVar("starterKitSelected"))
            {
                return;
            }

            if (EnableOneTimeSelectionLock && StarterKitSelectionStore.HasSelected(player))
            {
                return;
            }

            if (player.PlayerUI != null &&
                player.PlayerUI.windowManager != null &&
                player.PlayerUI.windowManager.IsWindowOpen("starterKitGroup"))
            {
                return;
            }

            player.PlayerUI.windowManager.Open("starterKitGroup", true);
        }

        /// <summary>
        /// ESC menüsünden starter kit menüsünü açmak için kullanılır.
        /// Kit seçimi zaten yapılmış oyuncular da menüyü görüntüleme amaçlı açabilir.
        /// </summary>
        public static void OpenStarterKitMenu()
        {
            var world = GameManager.Instance.World;
            if (world == null)
            {
                return;
            }

            var player = world.GetPrimaryPlayer();
            if (player == null)
            {
                return;
            }

            if (player.PlayerUI != null &&
                player.PlayerUI.windowManager != null)
            {
                player.PlayerUI.windowManager.Open("starterKitGroup", true);
            }
        }
    }
}