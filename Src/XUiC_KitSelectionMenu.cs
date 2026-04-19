using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Starter kit seçim menüsü controller'ı.
/// XML'deki event_onclick="eventChooseKit" ile çalışır.
/// </summary>
namespace StarterKits
{
    public class XUiC_KitSelectionMenu : XUiController
    {
        private sealed class KitOverviewData
        {
            public string DisplayName;
            public string Attributes;
            public string Bonuses;
            public string PreviewTitle;
        }

        private sealed class KitButtonEntry
        {
            public string KitName;
            public XUiC_SimpleButton Button;
            public XUiController SelectedOverlay;
        }

        private static readonly Dictionary<string, KitOverviewData> KitOverview = new Dictionary<string, KitOverviewData>(StringComparer.OrdinalIgnoreCase)
        {
            ["Scavenger"] = new KitOverviewData { DisplayName = "Scavenger", PreviewTitle = "Urban Scavenger", Attributes = "Fortitude, carry capacity, and loot sustain.", Bonuses = "Starts focused on longer scavenging runs, resource recovery, and staying stocked while exploring." },
            ["Huntsman"] = new KitOverviewData { DisplayName = "Huntsman", PreviewTitle = "Field Tracker", Attributes = "Perception, tracking, and ranged survival.", Bonuses = "Built around hunting efficiency, animal harvesting, and steady wilderness pressure." },
            ["Athlete"] = new KitOverviewData { DisplayName = "Athlete", PreviewTitle = "Mobility Focus", Attributes = "Agility, stamina flow, and recovery.", Bonuses = "Favours sprint uptime, evasive movement, and fast repositioning in the early game." },
            ["Tyson"] = new KitOverviewData { DisplayName = "Tyson", PreviewTitle = "Close Quarters", Attributes = "Strength, brawling, and direct damage.", Bonuses = "A bruiser-style opener with heavier melee pressure and stronger point-blank engagements." },
            ["Archer"] = new KitOverviewData { DisplayName = "Archer", PreviewTitle = "Precision Hunter", Attributes = "Perception, bow control, and precision damage.", Bonuses = "Leans into quiet kills, efficient ranged shots, and safer target picks at distance." },
            ["Farmer"] = new KitOverviewData { DisplayName = "Farmer", PreviewTitle = "Sustain Specialist", Attributes = "Fortitude, harvesting, and food stability.", Bonuses = "Optimized for farming loops, renewable supplies, and smoother long-term camp sustain." },
            ["Engineer"] = new KitOverviewData { DisplayName = "Engineer", PreviewTitle = "Tech Builder", Attributes = "Intellect, crafting, and trap support.", Bonuses = "Geared toward advanced utility, workstation progress, and stronger base setup tempo." },
            ["Ex-Soldier"] = new KitOverviewData { DisplayName = "Ex-Soldier", PreviewTitle = "Combat Veteran", Attributes = "Perception, toughness, and firearm readiness.", Bonuses = "A disciplined combat opener aimed at efficient firefights and dependable frontline pressure." },
            ["Doctor"] = new KitOverviewData { DisplayName = "Doctor", PreviewTitle = "Field Medic", Attributes = "Intellect, healing, and status recovery.", Bonuses = "Favours medical sustain, support utility, and faster stabilization after bad trades." },
            ["Miner"] = new KitOverviewData { DisplayName = "Miner", PreviewTitle = "Resource Breaker", Attributes = "Strength, harvesting speed, and ore yield.", Bonuses = "Centered on gathering momentum, faster material extraction, and stronger crafting feed." },
            ["Demoman"] = new KitOverviewData { DisplayName = "Demoman", PreviewTitle = "Explosive Expert", Attributes = "Perception, demolition, and burst damage.", Bonuses = "Pushes explosive utility, crowd control potential, and aggressive horde clear options." },
            ["Hitman"] = new KitOverviewData { DisplayName = "Hitman", PreviewTitle = "Silent Elimination", Attributes = "Agility, stealth, and critical damage.", Bonuses = "Ideal for surgical picks, stealth openings, and clean high-value target removals." },
            ["Dumb Luck"] = new KitOverviewData { DisplayName = "Dumb Luck", PreviewTitle = "Risk Reward", Attributes = "Luck-driven economy, looting, and swing potential.", Bonuses = "A volatile kit theme with stronger payoff spikes, gamble-heavy progression, and opportunistic gains." },
            ["Burglar"] = new KitOverviewData { DisplayName = "Burglar", PreviewTitle = "Infiltration", Attributes = "Agility, stealth entry, and loot access.", Bonuses = "Designed around silent movement, infiltration routes, and cleaner high-value loot runs." }
        };

        private static readonly string[] CandidateTextPropertyNames = { "Text", "Caption" };
        private static readonly string[] CandidateTextKeyPropertyNames = { "TextKey", "CaptionKey" };

        private readonly List<KitButtonEntry> kitButtons = new List<KitButtonEntry>();
        private string selectedKitName;
        private XUiController selectedKitNameLabel;
        private XUiController overviewHintLabel;
        private XUiController previewLabel;
        private XUiController attributesLabel;
        private XUiController bonusesLabel;
        private XUiController confirmButtonController;
        private XUiC_SimpleButton confirmButton;

        static XUiC_KitSelectionMenu()
        {
            // PlayerJoinedGame event handler'ını kaydetmek için sınıfı "uyandır".
            PlayerJoinedGameHandler.Init();
        }

        public override void Init()
        {
            base.Init();

            this.RegisterKitButton("Scavenger", "btnScavenger", "selScavenger");
            this.RegisterKitButton("Huntsman", "btnHuntsman", "selHuntsman");
            this.RegisterKitButton("Athlete", "btnAthlete", "selAthlete");
            this.RegisterKitButton("Tyson", "btnTyson", "selTyson");
            this.RegisterKitButton("Archer", "btnArcher", "selArcher");
            this.RegisterKitButton("Farmer", "btnFarmer", "selFarmer");
            this.RegisterKitButton("Engineer", "btnEngineer", "selEngineer");
            this.RegisterKitButton("Ex-Soldier", "btnExSoldier", "selExSoldier");
            this.RegisterKitButton("Doctor", "btnDoctor", "selDoctor");
            this.RegisterKitButton("Miner", "btnMiner", "selMiner");
            this.RegisterKitButton("Demoman", "btnDemoman", "selDemoman");
            this.RegisterKitButton("Hitman", "btnHitman", "selHitman");
            this.RegisterKitButton("Dumb Luck", "btnDumbLuck", "selDumbLuck");
            this.RegisterKitButton("Burglar", "btnBurglar", "selBurglar");

            this.selectedKitNameLabel = this.GetChildById("lblSelectedKitName");
            this.overviewHintLabel = this.GetChildById("lblOverviewHint");
            this.previewLabel = this.GetChildById("lblKitImagePlaceholder");
            this.attributesLabel = this.GetChildById("lblKitAttributes");
            this.bonusesLabel = this.GetChildById("lblKitBonuses");
            this.confirmButtonController = this.GetChildById("btnConfirmKit");
            this.confirmButton = this.confirmButtonController?.GetChildByType<XUiC_SimpleButton>();
            if (this.confirmButton != null)
            {
                this.confirmButton.OnPressed += this.OnConfirmButtonPressed;
            }

            this.ClearSelection();
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.ClearSelection();
        }

        /// <summary>
        /// XML'de tanımlanan event_onclick="eventChooseKit" burayı çağırır.
        /// </summary>
        public void eventChooseKit(XUiController sender, string _onClick)
        {
            string kitName = _onClick;

            if (string.IsNullOrEmpty(kitName))
            {
                return;
            }

            this.SelectKit(kitName);
        }

        private void RegisterKitButton(string kitName, string buttonId, string overlayId)
        {
            var buttonController = this.GetChildById(buttonId);
            var button = buttonController?.GetChildByType<XUiC_SimpleButton>();
            var overlay = this.GetChildById(overlayId);

            if (button == null || overlay == null)
            {
                Log.Warning($"[StarterKits] Failed to register kit button '{kitName}'. button={buttonId}, overlay={overlayId}");
                return;
            }

            button.OnPressed += this.OnKitButtonPressed;
            this.kitButtons.Add(new KitButtonEntry
            {
                KitName = kitName,
                Button = button,
                SelectedOverlay = overlay
            });
        }

        private void OnKitButtonPressed(XUiController sender, int mouseButton)
        {
            if (mouseButton != -1)
            {
                return;
            }

            var entry = this.FindKitButton(sender);
            if (entry == null)
            {
                return;
            }

            this.SelectKit(entry.KitName);
        }

        private KitButtonEntry FindKitButton(XUiController sender)
        {
            for (int i = 0; i < this.kitButtons.Count; i++)
            {
                if (ReferenceEquals(this.kitButtons[i].Button, sender))
                {
                    return this.kitButtons[i];
                }
            }

            return null;
        }

        private void SelectKit(string kitName)
        {
            this.selectedKitName = kitName;

            for (int i = 0; i < this.kitButtons.Count; i++)
            {
                bool isSelected = string.Equals(this.kitButtons[i].KitName, kitName, StringComparison.OrdinalIgnoreCase);
                if (this.kitButtons[i].SelectedOverlay?.ViewComponent != null)
                {
                    this.kitButtons[i].SelectedOverlay.ViewComponent.IsVisible = isSelected;
                }
            }

            this.UpdateOverview(kitName);
        }

        private void ClearSelection()
        {
            this.selectedKitName = null;

            for (int i = 0; i < this.kitButtons.Count; i++)
            {
                if (this.kitButtons[i].SelectedOverlay?.ViewComponent != null)
                {
                    this.kitButtons[i].SelectedOverlay.ViewComponent.IsVisible = false;
                }
            }

            this.UpdateOverview(null);
        }

        private void OnConfirmButtonPressed(XUiController sender, int mouseButton)
        {
            if (mouseButton != -1 || string.IsNullOrEmpty(this.selectedKitName))
            {
                return;
            }

            Log.Out($"[StarterKits] Confirm button pressed for kit '{this.selectedKitName}'.");
        }

        private void UpdateOverview(string kitName)
        {
            if (string.IsNullOrEmpty(kitName) || !KitOverview.TryGetValue(kitName, out KitOverviewData data))
            {
                this.SetText(this.selectedKitNameLabel, "No Kit Selected");
                this.SetText(this.overviewHintLabel, "Pick a starter kit on the left to preview its role, stat boosts, and bonuses.");
                this.SetText(this.previewLabel, "Select A Kit");
                this.SetText(this.attributesLabel, "Select a kit to inspect its attribute focus.");
                this.SetText(this.bonusesLabel, "Bonus details will appear here after you choose a kit.");
                this.SetEnabled(this.confirmButtonController, false);
                return;
            }

            this.SetText(this.selectedKitNameLabel, data.DisplayName);
            this.SetText(this.overviewHintLabel, "Review the preview, then lock in the kit when you're ready.");
            this.SetText(this.previewLabel, data.PreviewTitle);
            this.SetText(this.attributesLabel, data.Attributes);
            this.SetText(this.bonusesLabel, data.Bonuses);
            this.SetEnabled(this.confirmButtonController, true);
        }

        private void SetEnabled(XUiController controller, bool enabled)
        {
            if (controller?.ViewComponent == null)
            {
                return;
            }

            controller.ViewComponent.Enabled = enabled;
            controller.ViewComponent.IsDirty = true;
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

            controller.ViewComponent.IsDirty = true;
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

            if (player.Buffs.HasCustomVar("starterKitSelected"))
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