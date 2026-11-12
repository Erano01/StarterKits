using System;
using System.Collections.Generic;
using StarterKits.Kits;
using UnityEngine;

namespace StarterKits.Editor
{
    /// <summary>
    /// Main menu Starter Kit Editor (window "starterKitEditor", Config/XUi_Menu/windows.xml).
    /// Edits a working copy of the kit list; nothing touches disk until Save.
    /// </summary>
    public partial class XUiC_KitEditorWindow : XUiController
    {
        public static string ID = string.Empty;

        private const int KitRows = 14;
        private const int ProgRows = 8;
        private const int CatRows = 11;
        private const int ItemRows = 4;
        private const float ConfirmSeconds = 4f;

        private static readonly string[] BuiltInPortraits =
        {
            "Scavenger", "Huntsman", "Athlete", "Tyson", "Archer", "Farmer", "Engineer",
            "ExSoldier", "Doctor", "Miner", "Demoman", "Hitman", "DumbLuck", "Burglar"
        };

        private enum CatalogTab
        {
            All,
            Attributes,
            Perks,
            BookSets,
            Books,
            Magazines,
            Items
        }

        /// <summary>Perks tab filter, in the order of the catSub buttons.</summary>
        private static readonly string[] PerkAttributes =
        {
            "attPerception", "attStrength", "attFortitude", "attAgility", "attIntellect", "attGeneralPerks"
        };

        /// <summary>One line of the right-hand catalog list.</summary>
        private sealed class CatalogRow
        {
            public string Text;
            public string Icon;
            public string Atlas;
            public ProgressionInfo Progression;
            public ItemInfo Item;
            public bool IsHeader;
        }

        // ---- state ----
        private List<KitDefinition> kits = new List<KitDefinition>();
        private int selectedIndex = -1;
        private bool dirty;
        private int kitPage;
        private int progPage;
        private int catPage;
        private int itemPage;
        private CatalogTab catTab = CatalogTab.All;
        private string catSearch = string.Empty;
        private int perkAttribute;
        private readonly List<CatalogRow> catFiltered = new List<CatalogRow>();
        private string pendingConfirm;
        private float pendingConfirmUntil;

        // ---- views ----
        private XUiV_Label lblStatus;
        private XUiV_Label lblKitPage;
        private XUiV_Label lblImageInfo;
        private XUiV_Label lblProgHeader;
        private XUiV_Label lblProgPage;
        private XUiV_Label lblStatsInfo;
        private XUiV_Label lblCatPage;
        private XUiV_Label lblItemsHeader;
        private XUiV_Sprite sprKitImage;
        private XUiC_SimpleButton btnImageAtlas;
        private XUiC_SimpleButton btnStatsMode;
        private XUiC_SimpleButton btnDeleteKit;
        private XUiC_SimpleButton btnDuplicateKit;

        private XUiC_TextInput txtKitId;
        private XUiC_TextInput txtKitName;
        private XUiC_TextInput txtKitTitle;
        private XUiC_TextInput txtKitDesc;
        private XUiC_TextInput txtKitImage;
        private XUiC_TextInput txtCatSearch;
        private XUiC_TextInput txtItemName;

        private readonly XUiC_SimpleButton[] kitSlots = new XUiC_SimpleButton[KitRows];

        private readonly XUiV_Sprite[] progIcons = new XUiV_Sprite[ProgRows];
        private readonly XUiV_Label[] progNames = new XUiV_Label[ProgRows];
        private readonly XUiV_Label[] progLevels = new XUiV_Label[ProgRows];
        private readonly XUiC_SimpleButton[][] progButtons = new XUiC_SimpleButton[ProgRows][];

        private readonly XUiC_SimpleButton[] catTabs = new XUiC_SimpleButton[7];
        private readonly XUiC_SimpleButton[] catSubs = new XUiC_SimpleButton[6];
        private XUiV_Label lblCatSearch;
        private readonly XUiC_SimpleButton[] catRows = new XUiC_SimpleButton[CatRows];
        private readonly XUiV_Sprite[] catIcons = new XUiV_Sprite[CatRows];

        private readonly XUiV_Sprite[] itemIcons = new XUiV_Sprite[ItemRows];
        private readonly XUiV_Label[] itemNames = new XUiV_Label[ItemRows];
        private readonly XUiV_Label[] itemCounts = new XUiV_Label[ItemRows];
        private readonly XUiC_SimpleButton[] itemQualityButtons = new XUiC_SimpleButton[ItemRows];
        private readonly XUiC_SimpleButton[][] itemButtons = new XUiC_SimpleButton[ItemRows][];

        private KitDefinition SelectedKit =>
            selectedIndex >= 0 && selectedIndex < kits.Count ? kits[selectedIndex] : null;

        // =====================================================================
        // Lifecycle
        // =====================================================================

        public override void Init()
        {
            base.Init();
            ID = WindowGroup.Id;

            lblStatus = Label("lblStatus");
            lblKitPage = Label("lblKitPage");
            lblImageInfo = Label("lblImageInfo");
            lblProgHeader = Label("lblProgHeader");
            lblProgPage = Label("lblProgPage");
            lblStatsInfo = Label("lblStatsInfo");
            lblCatPage = Label("lblCatPage");
            lblItemsHeader = Label("lblItemsHeader");
            sprKitImage = Sprite("sprKitImage");

            // Kit list
            for (int i = 0; i < KitRows; i++)
            {
                int row = i;
                kitSlots[i] = Button("kitSlot" + i, (s, m) => OnKitSlot(row));
            }

            Button("btnKitPrev", (s, m) => { kitPage--; RefreshKitList(); });
            Button("btnKitNext", (s, m) => { kitPage++; RefreshKitList(); });
            Button("btnNewKit", (s, m) => OnNewKit());
            btnDuplicateKit = Button("btnDuplicateKit", (s, m) => OnDuplicateKit());
            btnDeleteKit = Button("btnDeleteKit", (s, m) => OnDeleteKit());
            Button("btnResetDefaults", (s, m) => OnResetDefaults());

            // Details
            txtKitId = TextInput("txtKitId", (text) => EditSelected(k => k.Id = text.Trim(), true));
            txtKitName = TextInput("txtKitName", (text) => EditSelected(k => k.Name = text, true));
            txtKitTitle = TextInput("txtKitTitle", (text) => EditSelected(k => k.Title = text, false));
            txtKitDesc = TextInput("txtKitDesc", (text) => EditSelected(k => k.Description = text, false));
            txtKitImage = TextInput("txtKitImage", (text) =>
            {
                EditSelected(k => k.Image = text.Trim(), false);
                RefreshImage();
            });

            btnImageAtlas = Button("btnImageAtlas", (s, m) => OnToggleImageAtlas());
            Button("btnImagePrev", (s, m) => OnCycleImage(-1));
            Button("btnImageNext", (s, m) => OnCycleImage(1));

            // Progressions
            Button("btnProgPrev", (s, m) => { progPage--; RefreshProgressions(); });
            Button("btnProgNext", (s, m) => { progPage++; RefreshProgressions(); });
            for (int i = 0; i < ProgRows; i++)
            {
                int row = i;
                progIcons[i] = Sprite("progIcon" + i);
                progNames[i] = Label("progName" + i);
                progLevels[i] = Label("progLevel" + i);
                progButtons[i] = new[]
                {
                    Button("progMinus" + i, (s, m) => OnProgLevel(row, -1)),
                    Button("progPlus" + i, (s, m) => OnProgLevel(row, 1)),
                    Button("progMax" + i, (s, m) => OnProgLevel(row, int.MaxValue)),
                    Button("progRemove" + i, (s, m) => OnProgRemove(row))
                };
            }

            btnStatsMode = Button("btnStatsMode", (s, m) => OnToggleStatsMode());

            // Catalog
            for (int i = 0; i < catTabs.Length; i++)
            {
                int tab = i;
                catTabs[i] = Button("catTab" + i, (s, m) =>
                {
                    catTab = (CatalogTab)tab;
                    catPage = 0;
                    if (catTab != CatalogTab.Items && txtItemName != null && catSearch == txtItemName.Text.Trim())
                    {
                        // Leaving item search: don't filter perks by an item name.
                        catSearch = string.Empty;
                        SetInput(txtCatSearch, string.Empty);
                    }

                    RefreshCatalog();
                });
            }

            for (int i = 0; i < catSubs.Length; i++)
            {
                int sub = i;
                catSubs[i] = Button("catSub" + i, (s, m) =>
                {
                    perkAttribute = sub;
                    catPage = 0;
                    RefreshCatalog();
                });
            }

            lblCatSearch = Label("lblCatSearch");
            txtCatSearch = TextInput("txtCatSearch", (text) =>
            {
                catSearch = text.Trim();
                catPage = 0;
                if (catTab == CatalogTab.Items)
                {
                    SetInput(txtItemName, catSearch);
                }

                RefreshCatalog();
            });

            for (int i = 0; i < CatRows; i++)
            {
                int row = i;
                catRows[i] = Button("catRow" + i, (s, m) => OnCatalogRow(row));
                catIcons[i] = Sprite("catIcon" + i);
            }

            Button("btnCatPrev", (s, m) => { catPage--; RefreshCatalog(); });
            Button("btnCatNext", (s, m) => { catPage++; RefreshCatalog(); });

            // Items
            // Typing an item name shows live suggestions (with icons) in the catalog list.
            txtItemName = TextInput("txtItemName", (text) =>
            {
                catTab = CatalogTab.Items;
                catSearch = text.Trim();
                catPage = 0;
                SetInput(txtCatSearch, catSearch);
                RefreshCatalog();
            });
            if (txtItemName != null)
            {
                txtItemName.OnSubmitHandler += (s, text) => OnAddItem();
            }

            Button("btnAddItem", (s, m) => OnAddItem());
            Button("btnItemPrev", (s, m) => { itemPage--; RefreshItems(); });
            Button("btnItemNext", (s, m) => { itemPage++; RefreshItems(); });
            for (int i = 0; i < ItemRows; i++)
            {
                int row = i;
                itemIcons[i] = Sprite("itemIcon" + i);
                itemNames[i] = Label("itemName" + i);
                itemCounts[i] = Label("itemCount" + i);
                itemQualityButtons[i] = Button("itemQuality" + i, (s, m) => OnItemQuality(row));
                itemButtons[i] = new[]
                {
                    Button("itemMinus" + i, (s, m) => OnItemCount(row, -1)),
                    Button("itemPlus" + i, (s, m) => OnItemCount(row, 1)),
                    itemQualityButtons[i],
                    Button("itemRemove" + i, (s, m) => OnItemRemove(row))
                };
            }

            // Footer
            Button("btnSave", (s, m) => OnSave());
            Button("btnRevert", (s, m) => OnRevert());
            Button("btnBack", (s, m) => OnBack());

            InitImport();
        }

        public override void OnOpen()
        {
            base.OnOpen();

            // ESC returns to the main menu, same as the vanilla Credits window.
            if (windowGroup.isModal)
            {
                windowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
            }

            ReloadFromDisk();
        }

        // =====================================================================
        // Loading / saving
        // =====================================================================

        private void ReloadFromDisk()
        {
            kits = KitRepository.LoadKits(out string message);
            dirty = false;
            kitPage = 0;
            pendingConfirm = null;
            SelectKit(kits.Count > 0 ? 0 : -1);
            RefreshCatalog();
            SetStatus(message);
        }

        private void OnSave()
        {
            string error = Validate();
            if (error != null)
            {
                SetStatus("Not saved: " + error);
                return;
            }

            foreach (KitDefinition kit in kits)
            {
                if (kit.AutoStats)
                {
                    RegenerateStats(kit);
                }
            }

            try
            {
                KitRepository.SaveKits(kits);
                dirty = false;
                SetStatus($"Saved {kits.Count} kit(s) to Kits/{KitRepository.UserFileName}.");
            }
            catch (Exception ex)
            {
                Log.Error($"[StarterKits] Saving kits failed: {ex}");
                SetStatus("Save failed: " + ex.Message);
            }

            RefreshAll();
        }

        private string Validate()
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < kits.Count; i++)
            {
                KitDefinition kit = kits[i];
                if (string.IsNullOrWhiteSpace(kit.Id))
                {
                    SelectKit(i);
                    return "a kit has an empty ID.";
                }

                if (!ids.Add(kit.Id))
                {
                    SelectKit(i);
                    return $"kit ID '{kit.Id}' is used twice.";
                }

                if (string.IsNullOrWhiteSpace(kit.Name))
                {
                    SelectKit(i);
                    return $"kit '{kit.Id}' has no name.";
                }
            }

            return null;
        }

        private void OnRevert()
        {
            if (dirty && !Confirm("revert", "Discard all unsaved changes? Press Revert again to confirm."))
            {
                return;
            }

            ReloadFromDisk();
        }

        private void OnBack()
        {
            if (dirty && !Confirm("back", "You have unsaved changes. Press Back again to leave without saving."))
            {
                return;
            }

            xui.playerUI.windowManager.Close(windowGroup.Id);
            xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
        }

        /// <summary>Two-press confirmation for destructive actions.</summary>
        private bool Confirm(string action, string prompt)
        {
            if (pendingConfirm == action && Time.realtimeSinceStartup <= pendingConfirmUntil)
            {
                pendingConfirm = null;
                return true;
            }

            pendingConfirm = action;
            pendingConfirmUntil = Time.realtimeSinceStartup + ConfirmSeconds;
            SetStatus(prompt);
            return false;
        }

        private void MarkDirty()
        {
            dirty = true;
            pendingConfirm = null;
        }

        // =====================================================================
        // Kit list
        // =====================================================================

        private void OnKitSlot(int row)
        {
            int index = kitPage * KitRows + row;
            if (index < kits.Count)
            {
                SelectKit(index);
            }
        }

        private void SelectKit(int index)
        {
            selectedIndex = index;
            progPage = 0;
            itemPage = 0;
            if (index >= 0)
            {
                kitPage = index / KitRows;
            }

            RefreshAll();
        }

        private void OnNewKit()
        {
            var kit = new KitDefinition
            {
                Id = UniqueId("NewKit"),
                Name = "New Kit",
                Title = "Custom Kit",
                Description = "Describe your kit here.",
                Image = "Scavenger",
                ImageAtlas = KitAtlases.ItemIcons,
                AutoStats = true
            };

            kits.Add(kit);
            MarkDirty();
            SelectKit(kits.Count - 1);
            SetStatus("New kit added. Pick progressions on the right, then Save.");
        }

        private void OnDuplicateKit()
        {
            KitDefinition source = SelectedKit;
            if (source == null)
            {
                return;
            }

            KitDefinition copy = source.Clone();
            copy.Id = UniqueId(source.Id + "_copy");
            copy.Name = source.Name + " (Copy)";
            kits.Insert(selectedIndex + 1, copy);
            MarkDirty();
            SelectKit(selectedIndex + 1);
            SetStatus($"Duplicated '{source.Name}'.");
        }

        private void OnDeleteKit()
        {
            KitDefinition kit = SelectedKit;
            if (kit == null)
            {
                return;
            }

            if (!Confirm("delete:" + kit.Id, $"Delete kit '{kit.Name}'? Press Delete again to confirm."))
            {
                return;
            }

            kits.RemoveAt(selectedIndex);
            MarkDirty();
            SelectKit(Math.Min(selectedIndex, kits.Count - 1));
            SetStatus($"Deleted '{kit.Name}'. Press Save to keep this change.");
        }

        private void OnResetDefaults()
        {
            if (!Confirm("reset", "Replace ALL kits with the 14 defaults? Press Reset All again to confirm."))
            {
                return;
            }

            kits = KitRepository.LoadDefaults(out string _);
            MarkDirty();
            kitPage = 0;
            SelectKit(kits.Count > 0 ? 0 : -1);
            SetStatus("Defaults restored. Press Save to keep them.");
        }

        private string UniqueId(string baseId)
        {
            string id = baseId;
            int n = 2;
            while (kits.Exists(k => string.Equals(k.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                id = baseId + n++;
            }

            return id;
        }

        // =====================================================================
        // Details
        // =====================================================================

        private void EditSelected(Action<KitDefinition> edit, bool refreshList)
        {
            KitDefinition kit = SelectedKit;
            if (kit == null)
            {
                return;
            }

            edit(kit);
            MarkDirty();
            if (refreshList)
            {
                RefreshKitList();
            }

            RefreshHeaderStatusIfIdle();
        }

        /// <summary>Item icons -> UI icons -> imported pictures -> item icons.</summary>
        private void OnToggleImageAtlas()
        {
            EditSelected(k =>
            {
                if (k.ImageAtlas == KitAtlases.ItemIcons)
                {
                    k.ImageAtlas = KitAtlases.Ui;
                }
                else if (k.ImageAtlas == KitAtlases.Ui)
                {
                    k.ImageAtlas = KitAtlases.File;
                }
                else
                {
                    k.ImageAtlas = KitAtlases.ItemIcons;
                }
            }, false);
            RefreshImage();
        }

        private void OnCycleImage(int direction)
        {
            KitDefinition kit = SelectedKit;
            if (kit == null)
            {
                return;
            }

            List<KeyValuePair<string, string>> images = ImageChoices();
            int current = images.FindIndex(c =>
                string.Equals(c.Key, kit.Image, StringComparison.OrdinalIgnoreCase) && c.Value == kit.ImageAtlas);
            int next = current < 0 ? 0 : (current + direction + images.Count) % images.Count;

            kit.Image = images[next].Key;
            kit.ImageAtlas = images[next].Value;
            MarkDirty();
            RefreshDetails();
        }

        /// <summary>(image, atlas): built-in portraits, imported pictures, and any image a kit already uses.</summary>
        private List<KeyValuePair<string, string>> ImageChoices()
        {
            var list = new List<KeyValuePair<string, string>>();
            void Add(string image, string atlas)
            {
                if (string.IsNullOrEmpty(image) ||
                    list.Exists(c => string.Equals(c.Key, image, StringComparison.OrdinalIgnoreCase) && c.Value == atlas))
                {
                    return;
                }

                list.Add(new KeyValuePair<string, string>(image, atlas));
            }

            foreach (string portrait in BuiltInPortraits)
            {
                Add(portrait, KitAtlases.ItemIcons);
            }

            foreach (string imported in KitImageStore.ListImages())
            {
                Add(imported, KitAtlases.File);
            }

            foreach (KitDefinition kit in kits)
            {
                if (kit.ImageAtlas == KitAtlases.ItemIcons || kit.ImageAtlas == KitAtlases.File)
                {
                    Add(kit.Image, kit.ImageAtlas);
                }
            }

            return list;
        }

        private void OnToggleStatsMode()
        {
            KitDefinition kit = SelectedKit;
            if (kit == null)
            {
                return;
            }

            if (!kit.AutoStats && kit.Stats.Count > 0 &&
                !Confirm("autostats:" + kit.Id, "Auto mode replaces this kit's hand-written stat lines. Press again to confirm."))
            {
                return;
            }

            kit.AutoStats = !kit.AutoStats;
            if (kit.AutoStats)
            {
                RegenerateStats(kit);
            }

            MarkDirty();
            RefreshStatsInfo();
        }

        private static void RegenerateStats(KitDefinition kit)
        {
            kit.Stats.Clear();
            kit.Stats.AddRange(KitStatBuilder.Build(kit));
        }

        // =====================================================================
        // Progressions
        // =====================================================================

        private void OnProgLevel(int row, int delta)
        {
            KitDefinition kit = SelectedKit;
            int index = progPage * ProgRows + row;
            if (kit == null || index >= kit.Progressions.Count)
            {
                return;
            }

            KitProgression p = kit.Progressions[index];
            int max = MaxLevelOf(p.Name);
            p.Level = delta == int.MaxValue ? max : KitRepository.Clamp(p.Level + delta, 1, max);
            OnKitContentChanged();
        }

        private void OnProgRemove(int row)
        {
            KitDefinition kit = SelectedKit;
            int index = progPage * ProgRows + row;
            if (kit == null || index >= kit.Progressions.Count)
            {
                return;
            }

            kit.Progressions.RemoveAt(index);
            OnKitContentChanged();
        }

        private void OnCatalogRow(int row)
        {
            KitDefinition kit = SelectedKit;
            int index = catPage * CatRows + row;
            if (kit == null || index >= catFiltered.Count)
            {
                return;
            }

            CatalogRow entry = catFiltered[index];
            if (entry.IsHeader)
            {
                return;
            }

            if (entry.Item != null)
            {
                AddItem(kit, entry.Item);
                return;
            }

            ProgressionInfo info = entry.Progression;
            if (info.Category == ProgressionCategory.BookSeries)
            {
                int added = 0;
                foreach (ProgressionInfo book in info.Books)
                {
                    if (kit.FindProgression(book.Name) == null)
                    {
                        kit.Progressions.Add(new KitProgression { Name = book.Name, Level = 1 });
                        added++;
                    }
                }

                SetStatus(added > 0 ? $"Added {added} book(s) from '{info.DisplayName}'." : $"'{info.DisplayName}' is already complete in this kit.");
            }
            else
            {
                KitProgression existing = kit.FindProgression(info.Name);
                if (existing != null)
                {
                    SetStatus($"'{info.DisplayName}' ({info.Path}) is already in this kit (level {existing.Level}).");
                    return;
                }

                int level = Math.Max(1, info.MaxLevel);
                kit.Progressions.Add(new KitProgression { Name = info.Name, Level = level });
                SetStatus($"Added '{info.DisplayName}' ({info.Path}) at level {level}. Use - / + to adjust.");
            }

            progPage = Math.Max(0, (kit.Progressions.Count - 1) / ProgRows);
            OnKitContentChanged();
        }

        private static int MaxLevelOf(string progressionName)
        {
            ProgressionInfo info = GameDataCatalog.FindProgression(progressionName);
            return info != null && info.MaxLevel > 0 ? info.MaxLevel : 100;
        }

        // =====================================================================
        // Items
        // =====================================================================

        private void OnAddItem()
        {
            KitDefinition kit = SelectedKit;
            if (kit == null || txtItemName == null)
            {
                return;
            }

            string name = txtItemName.Text.Trim();
            if (name.Length == 0)
            {
                SetStatus("Start typing an item name - suggestions appear in the catalog. Click one to add it.");
                return;
            }

            // Exact internal name, otherwise the best suggestion currently shown.
            ItemInfo info = GameDataCatalog.FindItem(name);
            if (info == null)
            {
                List<ItemInfo> matches = SearchItems(name);
                info = matches.Count > 0 ? matches[0] : null;
            }

            if (info == null)
            {
                SetStatus($"No item or block matches '{name}'.");
                return;
            }

            AddItem(kit, info);
        }

        private void AddItem(KitDefinition kit, ItemInfo info)
        {
            KitItem existing = kit.FindItem(info.Name);
            if (existing != null)
            {
                existing.Count++;
            }
            else
            {
                kit.Items.Add(new KitItem { Name = info.Name, Count = 1 });
            }

            itemPage = Math.Max(0, (kit.Items.Count - 1) / ItemRows);
            SetStatus(existing != null ? $"'{info.DisplayName}' count is now {existing.Count}." : $"Added '{info.DisplayName}' ({info.Name}).");
            OnKitContentChanged();
        }

        /// <summary>Items and blocks whose display or internal name contains the text; prefix matches first.</summary>
        private static List<ItemInfo> SearchItems(string text)
        {
            var prefix = new List<ItemInfo>();
            var contains = new List<ItemInfo>();
            foreach (ItemInfo item in GameDataCatalog.Items)
            {
                if (string.IsNullOrEmpty(text))
                {
                    contains.Add(item);
                }
                else if (item.DisplayName.StartsWith(text, StringComparison.OrdinalIgnoreCase) ||
                         item.Name.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                {
                    prefix.Add(item);
                }
                else if (item.DisplayName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                         item.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    contains.Add(item);
                }
            }

            prefix.AddRange(contains);
            return prefix;
        }

        private void OnItemCount(int row, int delta)
        {
            KitItem item = ItemAt(row);
            if (item == null)
            {
                return;
            }

            item.Count = KitRepository.Clamp(item.Count + delta, 1, 9999);
            OnKitContentChanged();
        }

        private void OnItemQuality(int row)
        {
            KitItem item = ItemAt(row);
            if (item == null)
            {
                return;
            }

            item.Quality = (item.Quality + 1) % 7;
            OnKitContentChanged();
        }

        private void OnItemRemove(int row)
        {
            KitDefinition kit = SelectedKit;
            int index = itemPage * ItemRows + row;
            if (kit == null || index >= kit.Items.Count)
            {
                return;
            }

            kit.Items.RemoveAt(index);
            OnKitContentChanged();
        }

        private KitItem ItemAt(int row)
        {
            KitDefinition kit = SelectedKit;
            int index = itemPage * ItemRows + row;
            return kit != null && index < kit.Items.Count ? kit.Items[index] : null;
        }

        private void OnKitContentChanged()
        {
            KitDefinition kit = SelectedKit;
            if (kit != null && kit.AutoStats)
            {
                RegenerateStats(kit);
            }

            MarkDirty();
            RefreshProgressions();
            RefreshItems();
            RefreshStatsInfo();
            RefreshCatalog();
        }

        // =====================================================================
        // Refresh
        // =====================================================================

        private void RefreshAll()
        {
            RefreshKitList();
            RefreshDetails();
            RefreshProgressions();
            RefreshItems();
            RefreshStatsInfo();
            RefreshCatalog();
        }

        private void RefreshKitList()
        {
            int pages = Math.Max(1, (kits.Count + KitRows - 1) / KitRows);
            kitPage = KitRepository.Clamp(kitPage, 0, pages - 1);
            for (int i = 0; i < KitRows; i++)
            {
                int index = kitPage * KitRows + i;
                XUiC_SimpleButton slot = kitSlots[i];
                if (slot == null)
                {
                    continue;
                }

                bool used = index < kits.Count;
                SetVisible(slot, used);
                if (!used)
                {
                    continue;
                }

                KitDefinition kit = kits[index];
                bool selected = index == selectedIndex;
                slot.Text = selected ? "> " + DisplayNameOf(kit) + " <" : DisplayNameOf(kit);
                if (slot.Button != null)
                {
                    slot.Button.Selected = selected;
                }
            }

            SetLabel(lblKitPage, $"{kitPage + 1} / {pages}  ({kits.Count} kits)");
            bool hasKit = SelectedKit != null;
            if (btnDeleteKit != null)
            {
                btnDeleteKit.Enabled = hasKit;
            }

            if (btnDuplicateKit != null)
            {
                btnDuplicateKit.Enabled = hasKit;
            }
        }

        private static string DisplayNameOf(KitDefinition kit)
        {
            return string.IsNullOrWhiteSpace(kit.Name) ? "(" + kit.Id + ")" : kit.Name;
        }

        private void RefreshDetails()
        {
            KitDefinition kit = SelectedKit;
            SetInput(txtKitId, kit?.Id);
            SetInput(txtKitName, kit?.Name);
            SetInput(txtKitTitle, kit?.Title);
            SetInput(txtKitDesc, kit?.Description);
            SetInput(txtKitImage, kit?.Image);
            RefreshImage();
        }

        private void RefreshImage()
        {
            KitDefinition kit = SelectedKit;
            bool hasImage = kit != null && !string.IsNullOrEmpty(kit.Image);
            bool isFile = hasImage && kit.ImageAtlas == KitAtlases.File;

            if (sprKitImage != null)
            {
                if (hasImage && !isFile)
                {
                    sprKitImage.UIAtlas = kit.ImageAtlas;
                    sprKitImage.SpriteName = kit.Image;
                }

                sprKitImage.IsVisible = hasImage && !isFile;
            }

            Texture2D portrait = isFile ? KitImageStore.GetPortrait(kit.Image) : null;
            if (texKitImageFile != null)
            {
                texKitImageFile.Texture = portrait;
                texKitImageFile.IsVisible = portrait != null;
                texKitImageFile.SetDirty();
            }

            if (btnImageAtlas != null)
            {
                string source = kit == null ? string.Empty
                    : kit.ImageAtlas == KitAtlases.Ui ? "UI icons"
                    : kit.ImageAtlas == KitAtlases.File ? "Imported"
                    : "Item icons";
                btnImageAtlas.Text = source;
            }

            string info = string.Empty;
            if (kit != null)
            {
                info = isFile && portrait == null ? $"Missing: Kits/Images/{kit.Image}.png" : "Built-in + imported";
            }

            SetLabel(lblImageInfo, info);
        }

        private void RefreshProgressions()
        {
            KitDefinition kit = SelectedKit;
            int count = kit?.Progressions.Count ?? 0;
            int pages = Math.Max(1, (count + ProgRows - 1) / ProgRows);
            progPage = KitRepository.Clamp(progPage, 0, pages - 1);

            SetLabel(lblProgHeader, $"Granted progressions ({count})");
            SetLabel(lblProgPage, $"{progPage + 1} / {pages}");

            for (int i = 0; i < ProgRows; i++)
            {
                int index = progPage * ProgRows + i;
                bool used = kit != null && index < count;
                SetRowVisible(progIcons[i], progNames[i], progLevels[i], progButtons[i], used);
                if (!used)
                {
                    continue;
                }

                KitProgression p = kit.Progressions[index];
                ProgressionInfo info = GameDataCatalog.FindProgression(p.Name);
                if (info != null)
                {
                    SetLabel(progNames[i], $"{info.DisplayName}  [{info.Path}]");
                    SetLabel(progLevels[i], $"{p.Level} / {info.MaxLevel}");
                    SetIcon(progIcons[i], KitAtlases.Ui, info.Icon);
                }
                else
                {
                    SetLabel(progNames[i], $"{p.Name}  [unknown]");
                    SetLabel(progLevels[i], p.Level.ToString());
                    SetIcon(progIcons[i], KitAtlases.Ui, "ui_game_symbol_modded");
                }
            }
        }

        private void RefreshItems()
        {
            KitDefinition kit = SelectedKit;
            int count = kit?.Items.Count ?? 0;
            int pages = Math.Max(1, (count + ItemRows - 1) / ItemRows);
            itemPage = KitRepository.Clamp(itemPage, 0, pages - 1);
            SetLabel(lblItemsHeader, $"Items ({count})  {itemPage + 1}/{pages}");

            for (int i = 0; i < ItemRows; i++)
            {
                int index = itemPage * ItemRows + i;
                bool used = kit != null && index < count;
                SetRowVisible(itemIcons[i], itemNames[i], itemCounts[i], itemButtons[i], used);
                if (!used)
                {
                    continue;
                }

                KitItem item = kit.Items[index];
                ItemInfo info = GameDataCatalog.FindItem(item.Name);
                SetLabel(itemNames[i], info != null ? info.DisplayName : item.Name + " [unknown]");
                SetLabel(itemCounts[i], "x" + item.Count);
                SetIcon(itemIcons[i], KitAtlases.ItemIcons, info != null ? info.Icon : item.Name);
                if (itemQualityButtons[i] != null)
                {
                    itemQualityButtons[i].Text = item.Quality > 0 ? "Quality " + item.Quality : "Quality: auto";
                }
            }
        }

        private void RefreshStatsInfo()
        {
            KitDefinition kit = SelectedKit;
            if (btnStatsMode != null)
            {
                btnStatsMode.Text = kit == null ? string.Empty : (kit.AutoStats ? "Stat lines: Auto" : "Stat lines: Custom");
                btnStatsMode.Enabled = kit != null;
            }

            if (kit == null)
            {
                SetLabel(lblStatsInfo, string.Empty);
                return;
            }

            int lines = kit.Stats.Count;
            string info = kit.AutoStats ? $"{lines} line(s), built from the list" : $"{lines} hand-written line(s)";
            if (lines > KitStatBuilder.MaxVisibleLines)
            {
                info += $" - only {KitStatBuilder.MaxVisibleLines} fit in game";
            }

            SetLabel(lblStatsInfo, info);
        }

        private void RefreshCatalog()
        {
            BuildCatalogRows();

            int pages = Math.Max(1, (catFiltered.Count + CatRows - 1) / CatRows);
            catPage = KitRepository.Clamp(catPage, 0, pages - 1);
            string unit = catTab == CatalogTab.Items ? "items" : "entries";
            SetLabel(lblCatPage, $"{catPage + 1} / {pages}  ({CountSelectable()} {unit})");
            SetLabel(lblCatSearch, catTab == CatalogTab.Items ? "Item" : "Search");

            for (int i = 0; i < catTabs.Length; i++)
            {
                if (catTabs[i]?.Button != null)
                {
                    catTabs[i].Button.Selected = i == (int)catTab;
                }
            }

            for (int i = 0; i < catSubs.Length; i++)
            {
                SetVisible(catSubs[i], catTab == CatalogTab.Perks);
                if (catSubs[i]?.Button != null)
                {
                    catSubs[i].Button.Selected = i == perkAttribute;
                }
            }

            for (int i = 0; i < CatRows; i++)
            {
                int index = catPage * CatRows + i;
                bool used = index < catFiltered.Count;
                SetVisible(catRows[i], used);
                if (catIcons[i] != null)
                {
                    catIcons[i].IsVisible = false;
                }

                if (!used || catRows[i] == null)
                {
                    continue;
                }

                CatalogRow row = catFiltered[index];
                catRows[i].Text = row.Text;
                catRows[i].Enabled = !row.IsHeader;
                if (!row.IsHeader)
                {
                    SetIcon(catIcons[i], row.Atlas, row.Icon);
                }
            }
        }

        private int CountSelectable()
        {
            int count = 0;
            foreach (CatalogRow row in catFiltered)
            {
                if (!row.IsHeader)
                {
                    count++;
                }
            }

            return count;
        }

        private void BuildCatalogRows()
        {
            catFiltered.Clear();
            KitDefinition kit = SelectedKit;

            switch (catTab)
            {
                case CatalogTab.Items:
                    foreach (ItemInfo item in SearchItems(catSearch))
                    {
                        KitItem owned = kit?.FindItem(item.Name);
                        string tag = item.Kind;
                        string added = owned != null ? $"   [x{owned.Count} in kit]" : string.Empty;
                        catFiltered.Add(new CatalogRow
                        {
                            Text = $"        {item.DisplayName}   -   {tag}: {item.Name}{added}",
                            Icon = item.Icon,
                            Atlas = KitAtlases.ItemIcons,
                            Item = item
                        });
                    }

                    return;

                case CatalogTab.Perks:
                    // Attribute > skill category > perk, like the in-game skill screen.
                    ProgressionInfo attribute = GameDataCatalog.FindProgression(PerkAttributes[perkAttribute]);
                    foreach (ProgressionInfo group in GameDataCatalog.Progressions)
                    {
                        if (group.Category != ProgressionCategory.Skill || group.Attribute != attribute)
                        {
                            continue;
                        }

                        AddGroup(GameDataCatalog.ShortGroupName(group) + " Perks", group, ProgressionCategory.Perk, kit);
                    }

                    return;

                case CatalogTab.Books:
                    foreach (ProgressionInfo set in GameDataCatalog.Progressions)
                    {
                        if (set.Category == ProgressionCategory.BookSeries)
                        {
                            AddGroup(set.DisplayName, set, ProgressionCategory.Book, kit);
                        }
                    }

                    return;
            }

            foreach (ProgressionInfo info in GameDataCatalog.Progressions)
            {
                if (MatchesTab(info) && MatchesSearch(info))
                {
                    catFiltered.Add(ProgressionRow(info, kit, true));
                }
            }
        }

        /// <summary>A non-clickable header followed by the matching children of that group.</summary>
        private void AddGroup(string header, ProgressionInfo group, ProgressionCategory childCategory, KitDefinition kit)
        {
            bool headerAdded = false;
            foreach (ProgressionInfo info in GameDataCatalog.Progressions)
            {
                if (info.Category != childCategory || info.Group != group || !MatchesSearch(info))
                {
                    continue;
                }

                if (!headerAdded)
                {
                    catFiltered.Add(new CatalogRow { Text = $"---  {header}  ---", IsHeader = true });
                    headerAdded = true;
                }

                catFiltered.Add(ProgressionRow(info, kit, false));
            }
        }

        private static CatalogRow ProgressionRow(ProgressionInfo info, KitDefinition kit, bool showPath)
        {
            string range = info.Category == ProgressionCategory.BookSeries ? $"{info.MaxLevel} books" : $"max {info.MaxLevel}";
            string where = showPath && !string.IsNullOrEmpty(info.Path) ? info.Path + ", " : string.Empty;
            string added = kit != null && IsInKit(kit, info) ? "   [added]" : string.Empty;
            return new CatalogRow
            {
                Text = $"        {info.DisplayName}   -   {where}{range}{added}",
                Icon = info.Icon,
                Atlas = KitAtlases.Ui,
                Progression = info
            };
        }

        private static bool IsInKit(KitDefinition kit, ProgressionInfo info)
        {
            if (info.Category != ProgressionCategory.BookSeries)
            {
                return kit.FindProgression(info.Name) != null;
            }

            foreach (ProgressionInfo book in info.Books)
            {
                if (kit.FindProgression(book.Name) == null)
                {
                    return false;
                }
            }

            return info.Books.Count > 0;
        }

        private bool MatchesTab(ProgressionInfo info)
        {
            // Skills (category headers, max 1) and hidden 0-level attributes cannot be meaningfully granted.
            if (info.Category == ProgressionCategory.Skill || info.MaxLevel <= 0)
            {
                return false;
            }

            switch (catTab)
            {
                case CatalogTab.Attributes: return info.Category == ProgressionCategory.Attribute;
                case CatalogTab.BookSets: return info.Category == ProgressionCategory.BookSeries;
                case CatalogTab.Magazines: return info.Category == ProgressionCategory.Crafting;
                default: return true;
            }
        }

        private bool MatchesSearch(ProgressionInfo info)
        {
            if (string.IsNullOrEmpty(catSearch))
            {
                return true;
            }

            return info.DisplayName.IndexOf(catSearch, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   info.Name.IndexOf(catSearch, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   info.Path.IndexOf(catSearch, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string CategoryLabel(ProgressionCategory category)
        {
            switch (category)
            {
                case ProgressionCategory.Attribute: return "Attribute";
                case ProgressionCategory.Skill: return "Skill";
                case ProgressionCategory.Perk: return "Perk";
                case ProgressionCategory.BookSeries: return "Book set";
                case ProgressionCategory.Book: return "Book";
                case ProgressionCategory.Crafting: return "Magazine";
                default: return category.ToString();
            }
        }

        private void RefreshHeaderStatusIfIdle()
        {
            if (pendingConfirm == null && dirty)
            {
                SetStatus("Unsaved changes.");
            }
        }

        // =====================================================================
        // View helpers
        // =====================================================================

        private void SetStatus(string text)
        {
            SetLabel(lblStatus, dirty && !string.IsNullOrEmpty(text) && !text.StartsWith("Saved") ? text + "  (unsaved)" : text);
        }

        private XUiController Find(string id)
        {
            XUiController c = GetChildById(id);
            if (c == null)
            {
                Log.Warning($"[StarterKits] Kit editor: control '{id}' not found.");
            }

            return c;
        }

        private XUiV_Label Label(string id) => Find(id)?.ViewComponent as XUiV_Label;

        private XUiV_Sprite Sprite(string id) => Find(id)?.ViewComponent as XUiV_Sprite;

        private XUiC_SimpleButton Button(string id, XUiEvent_OnPressEventHandler onPress)
        {
            XUiController c = Find(id);
            XUiC_SimpleButton button = c as XUiC_SimpleButton ?? c?.GetChildByType<XUiC_SimpleButton>();
            if (button != null && onPress != null)
            {
                button.OnPressed += onPress;
            }

            return button;
        }

        private XUiC_TextInput TextInput(string id, Action<string> onUserChange)
        {
            XUiController c = Find(id);
            XUiC_TextInput input = c as XUiC_TextInput ?? c?.GetChildByType<XUiC_TextInput>();
            if (input != null && onUserChange != null)
            {
                input.OnChangeHandler += (sender, text, changeFromCode) =>
                {
                    if (!changeFromCode)
                    {
                        onUserChange(text ?? string.Empty);
                    }
                };
            }

            return input;
        }

        private static void SetInput(XUiC_TextInput input, string value)
        {
            if (input == null)
            {
                return;
            }

            string text = value ?? string.Empty;
            if (input.Text != text)
            {
                input.Text = text;
            }
        }

        private static void SetLabel(XUiV_Label label, string text)
        {
            if (label != null)
            {
                label.Text = text ?? string.Empty;
            }
        }

        private static void SetIcon(XUiV_Sprite sprite, string atlas, string spriteName)
        {
            if (sprite == null)
            {
                return;
            }

            bool show = !string.IsNullOrEmpty(spriteName);
            if (show)
            {
                sprite.UIAtlas = atlas;
                sprite.SpriteName = spriteName;
            }

            sprite.IsVisible = show;
        }

        private static void SetVisible(XUiController controller, bool visible)
        {
            if (controller?.ViewComponent != null)
            {
                controller.ViewComponent.IsVisible = visible;
            }
        }

        private static void SetRowVisible(XUiV_Sprite icon, XUiV_Label name, XUiV_Label value, XUiC_SimpleButton[] buttons, bool visible)
        {
            if (icon != null)
            {
                icon.IsVisible = visible;
            }

            if (name != null)
            {
                name.IsVisible = visible;
            }

            if (value != null)
            {
                value.IsVisible = visible;
            }

            if (buttons == null)
            {
                return;
            }

            foreach (XUiC_SimpleButton button in buttons)
            {
                SetVisible(button, visible);
            }
        }
    }
}
