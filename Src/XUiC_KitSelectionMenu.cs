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
            public string Description;
            public string PreviewTitle;
            public string[] StatLines;
        }

        private sealed class KitButtonEntry
        {
            public string KitName;
            public XUiC_SimpleButton Button;
            public XUiController SelectedOverlay;
        }

        private static readonly Dictionary<string, KitOverviewData> KitOverview = new Dictionary<string, KitOverviewData>(StringComparer.OrdinalIgnoreCase)
        {
            ["Scavenger"] = new KitOverviewData
            {
                DisplayName = "Scavenger",
                PreviewTitle = "Urban Scavenger",
                Description = "One man's trash is another man's entire inventory. You built a life out of what others stepped over.",
                StatLines = new[] { "Salvage Operations 5/5", "Perception Mastery 2/5", "Salvage Tools 4/75" }
            },
            ["Huntsman"] = new KitOverviewData
            {
                DisplayName = "Huntsman",
                PreviewTitle = "Field Tracker",
                Description = "The wasteland is your hunting ground. You don't miss. You don't get hungry. They never hear you coming.",
                StatLines = new[] { "Dead Eye 5/5", "Animal Tracker 5/5", "The Penetrator 5/5", "The Huntsman 5/5", "Iron Gut 5/5", "Hidden Strike 2/5", "Sniper Perk Book 7/7", "Rifles 26/100" }
            },
            ["Athlete"] = new KitOverviewData
            {
                DisplayName = "Athlete",
                PreviewTitle = "Mobility Focus",
                Description = "You're not running away. You're tactically relocating. Very fast. Every single time.",
                StatLines = new[] { "Parkour 5/5", "Hard Target 5/5", "Run and Gun 3/5", "Armor Crafting Skill 11/100", "Rule 1: Cardio 5/5" }
            },
            ["Tyson"] = new KitOverviewData
            {
                DisplayName = "Tyson",
                PreviewTitle = "Close Quarters",
                Description = "No guns. No problem. Your fists are the only weapon the apocalypse couldn't rust.",
                StatLines = new[] { "The Brawler 5/5", "Lightning Hands 5/5", "Pain Tolerance 5/5", "Fortitude Mastery 5/5", "Siphoning Strikes 5/5", "Healing Factor 5/5", "Iron Gut 5/5", "Bar Brawling 7/7", "Knuckles 11/75" }
            },
            ["Archer"] = new KitOverviewData
            {
                DisplayName = "Archer",
                PreviewTitle = "Precision Hunter",
                Description = "Silent. Precise. Deeply judged by everyone until the horde drops. Then suddenly everyone loves the bow guy.",
                StatLines = new[] { "Archery 5/5", "From the Shadows 2/5", "Hidden Strike 3/5", "Ranger's Guide to Archery 7/7", "Bows 11/75" }
            },
            ["Farmer"] = new KitOverviewData
            {
                DisplayName = "Farmer",
                PreviewTitle = "Sustain Specialist",
                Description = "While others loot, you grow. Dirt under your nails, spear in your hand, food on everyone's table.",
                StatLines = new[] { "Living off The Land 3/3", "Armor Crafting Skill 11/100", "Seeds Crafting Skill 20/20", "Food Crafting Skill 100/100", "Super Corn Crafting Magazine (Automatically Readed)", "Fullset Farmer Armor", "Spear Master 5/5", "Quick and Perceptive 5/5", "Spear Hunter 7/7", "Spear Crafting Skill 11/75" }
            },
            ["Engineer"] = new KitOverviewData
            {
                DisplayName = "Engineer",
                PreviewTitle = "Tech Builder",
                Description = "Turrets, vehicles, electricity - if it runs on volts or wheels, you built it. The apocalypse has a power grid now. Yours.",
                StatLines = new[] { "Electrician Crafting Skills 55/100", "Vehicles Crafting Skill 20/100", "Robotics 76/100", "Tech Junkie 7/7", "Workstations 34/75", "Advanced Engineering 5/5", "Grease Monkey 5/5" }
            },
            ["Ex-Soldier"] = new KitOverviewData
            {
                DisplayName = "Ex-Soldier",
                PreviewTitle = "Combat Veteran",
                Description = "Old rank, new rules. Keep moving, keep firing. Discipline is the last thing that survived.",
                StatLines = new[] { "Machine Gunner 5/5", "Run and Gun 5/5", "Commando Armor Fullset", "Armor Crafting Skill 11/100", "Medium Armor 4/4", "Urban Combat 7/7", "The Automatic Weapon Handbook 7/7", "Machine Guns 11/100" }
            },
            ["Doctor"] = new KitOverviewData
            {
                DisplayName = "Doctor",
                PreviewTitle = "Field Medic",
                Description = "Someone has to keep the idiots alive. Might as well be you.",
                StatLines = new[] { "Physician 5/5", "Charismatic Nature 3/5", "Medical 75/75", "Foot 27/100" }
            },
            ["Miner"] = new KitOverviewData
            {
                DisplayName = "Miner",
                PreviewTitle = "Resource Breaker",
                Description = "You hit rocks for a living. Zombies are just rocks that bleed.",
                StatLines = new[] { "Skull Crusher 5/5", "Grand Slam 5/5", "Strength Master 5/5", "Pack Mule 5/5", "Miner 69'er 5/5", "Mother Lode 5/5", "Sledge Saga 7/7", "Harvesting Tools 11/100", "Sledge Hammer 11/75", "Workstations 34/75" }
            },
            ["Demoman"] = new KitOverviewData
            {
                DisplayName = "Demoman",
                PreviewTitle = "Explosive Expert",
                Description = "Collateral damage is just a fancy word for fun. If it's still standing, use a bigger one.",
                StatLines = new[] { "Demolition Expert 5/5", "The Infiltrator 5/5", "Explosives 50/100" }
            },
            ["Hitman"] = new KitOverviewData
            {
                DisplayName = "Hitman",
                PreviewTitle = "Silent Elimination",
                Description = "Clean. Quiet. Gone before they hit the floor. Getting paid in cans these days, but the craft stays the same.",
                StatLines = new[] { "Gunslinger 5/5", "Run and Gun 5/5", "Agility Mastery 2/5", "Hidden Strike 5/5", "From the Shadows 5/5", "Pistol Pete 7/7", "Magnum Enforcer 7/7", "Handguns 26/100" }
            },
            ["Dumb Luck"] = new KitOverviewData
            {
                DisplayName = "Dumb Luck",
                PreviewTitle = "Risk Reward",
                Description = "You're not skilled. You're not smart. But somehow you always find the good stuff. Don't question it.",
                StatLines = new[] { "Lucky Looter (Skill) 5/5", "Lucky Looter (Perk Book) 7/7", "Wasteland Treasures 7/7", "The Daring Adventurer 5/5", "Treasure Hunter 5/5" }
            },
            ["Burglar"] = new KitOverviewData
            {
                DisplayName = "Burglar",
                PreviewTitle = "Infiltration",
                Description = "Why fight when you can just... take it? Locks open, shadows hide, blades finish the conversation.",
                StatLines = new[] { "Better Barter 5/5", "Lock Picking 3/3", "Workstations 12/75", "Blades 36/75", "Deep Cuts 5/5", "Hidden Strike 3/5", "From the Shadows 3/5", "Whirlwind 5/5" }
            }
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
                this.SetText(this.overviewHintLabel, "Pick a starter kit on the left to preview its description and perk stats.");
                this.SetText(this.previewLabel, "Select A Kit");
                this.SetText(this.attributesLabel, "Select a kit to inspect its description.");
                this.SetText(this.bonusesLabel, "Select a kit to see its perk and skill levels.");
                this.SetEnabled(this.confirmButtonController, false);
                return;
            }

            this.SetText(this.selectedKitNameLabel, data.DisplayName);
            this.SetText(this.overviewHintLabel, "Review the preview, then lock in the kit when you're ready.");
            this.SetText(this.previewLabel, data.PreviewTitle);
            this.SetText(this.attributesLabel, data.Description);
            this.SetText(this.bonusesLabel, this.BuildStatsText(data));
            this.SetEnabled(this.confirmButtonController, true);
        }

        private string BuildStatsText(KitOverviewData data)
        {
            if (data?.StatLines == null || data.StatLines.Length == 0)
            {
                return string.Empty;
            }

            return "- " + string.Join("\n- ", data.StatLines);
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