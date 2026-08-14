using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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

        private sealed class KitOverviewData
        {
            public string DisplayName;
            public string Description;
            public string PreviewTitle;
            public StatEntry[] StatLines;
        }

        private sealed class StatEntry
        {
            public string Text;
            public string IconSpriteName;

            public StatEntry(string text, string iconSpriteName = null)
            {
                Text = text;
                IconSpriteName = iconSpriteName;
            }
        }

        private sealed class KitButtonEntry
        {
            public string KitName;
            public XUiC_SimpleButton Button;
            public XUiController SelectedOverlay;
        }

        private sealed class KitPreviewImageEntry
        {
            public string KitName;
            public XUiController TextureController;
        }

        private sealed class KitRewardData
        {
            public Dictionary<string, int> ProgressionFloors;
            public Dictionary<string, float> CustomVars;
            public Dictionary<string, int> ItemRewards;
        }

        private static readonly Dictionary<string, KitOverviewData> KitOverview = new Dictionary<string, KitOverviewData>(StringComparer.OrdinalIgnoreCase)
        {
            ["Scavenger"] = new KitOverviewData
            {
                DisplayName = "Scavenger",
                PreviewTitle = "Urban Scavenger",
                Description = "One man's trash is another man's entire inventory. You built a life out of what others stepped over.",
                StatLines = new[] { new StatEntry("Salvage Operations 5/5", "ui_game_symbol_scrap"), new StatEntry("Perception Mastery 2/5", "ui_game_symbol_perception_mastery"), new StatEntry("Salvage Tools 4/75", "ui_game_symbol_wrench") }
            },
            ["Huntsman"] = new KitOverviewData
            {
                DisplayName = "Huntsman",
                PreviewTitle = "Field Tracker",
                Description = "The wasteland is your hunting ground. You don't miss. You don't get hungry. They never hear you coming.",
                StatLines = new[] { new StatEntry("Dead Eye 5/5", "ui_game_symbol_map_cursor"), new StatEntry("Animal Tracker 5/5", "ui_game_symbol_animal_tracker"), new StatEntry("The Penetrator 5/5", "ui_game_symbol_paint_copy_block"), new StatEntry("The Huntsman 5/5", "ui_game_symbol_archery"), new StatEntry("Iron Gut 5/5"), new StatEntry("Hidden Strike 2/5", "ui_game_symbol_sneak_attack"), new StatEntry("Sniper Perk Book 7/7", "ui_game_symbol_long_shot"), new StatEntry("Rifles 26/100", "ui_game_symbol_long_shot") }
            },
            ["Athlete"] = new KitOverviewData
            {
                DisplayName = "Athlete",
                PreviewTitle = "Mobility Focus",
                Description = "You're not running away. You're tactically relocating. Very fast. Every single time.",
                StatLines = new[] { new StatEntry("Parkour 5/5", "ui_game_symbol_parkour"), new StatEntry("Hard Target 5/5", "ui_game_symbol_hard_target"), new StatEntry("Run and Gun 3/5", "ui_game_symbol_run_and_gun"), new StatEntry("Armor Skill 11/100", "ui_game_symbol_armor_iron"), new StatEntry("Rule 1: Cardio 5/5", "ui_game_symbol_cardio") }
            },
            ["Tyson"] = new KitOverviewData
            {
                DisplayName = "Tyson",
                PreviewTitle = "Close Quarters",
                Description = "No guns. No problem. Your fists are the only weapon the apocalypse couldn't rust.",
                StatLines = new[] { new StatEntry("The Brawler 5/5", "ui_game_symbol_boxer"), new StatEntry("Lightning Hands 5/5", "ui_game_symbol_knunchuck"), new StatEntry("Pain Tolerance 5/5", "ui_game_symbol_character"), new StatEntry("Fortitude Mastery 5/5", "ui_game_symbol_fortitude_mastery"), new StatEntry("Siphoning Strikes 5/5", "ui_game_symbol_siphoning_strikes"), new StatEntry("Healing Factor 5/5", "ui_game_symbol_healing_factor"), new StatEntry("Iron Gut 5/5"), new StatEntry("Bar Brawling 7/7", "ui_game_symbol_boxer"), new StatEntry("Knuckles 11/75", "ui_game_symbol_boxer") }
            },
            ["Archer"] = new KitOverviewData
            {
                DisplayName = "Archer",
                PreviewTitle = "Precision Hunter",
                Description = "Silent. Precise. Deeply judged by everyone until the horde drops. Then suddenly everyone loves the bow guy.",
                StatLines = new[] { new StatEntry("Archery 5/5", "ui_game_symbol_archery"), new StatEntry("From the Shadows 2/5", "ui_game_symbol_bat"), new StatEntry("Hidden Strike 3/5", "ui_game_symbol_sneak_attack"), new StatEntry("Ranger's Guide 7/7", "ui_game_symbol_archery"), new StatEntry("Bows 11/75", "ui_game_symbol_archery") }
            },
            ["Farmer"] = new KitOverviewData
            {
                DisplayName = "Farmer",
                PreviewTitle = "Sustain Specialist",
                Description = "While others loot, you grow. Dirt under your nails, spear in your hand, food on everyone's table.",
                StatLines = new[] { new StatEntry("Living off The Land 3/3", "ui_game_symbol_tree"), new StatEntry("Armor Skill 11/100", "ui_game_symbol_armor_iron"), new StatEntry("Seeds Skill 20/20", "ui_game_symbol_crops"), new StatEntry("Food Skill 100/100", "ui_game_symbol_fork"), new StatEntry("Super Corn Crafting"), new StatEntry("Medium Armor 4/4", "ui_game_symbol_light_armor2"), new StatEntry("Fullset Farmer Outfit"), new StatEntry("Spear Master 5/5", "ui_game_symbol_spear"), new StatEntry("Quick and Perceptive 5/5", "ui_game_symbol_knunchuck"), new StatEntry("Spear Hunter 7/7", "ui_game_symbol_spear"), new StatEntry("Spear Skill 11/75", "ui_game_symbol_spear") }
            },
            ["Engineer"] = new KitOverviewData
            {
                DisplayName = "Engineer",
                PreviewTitle = "Tech Builder",
                Description = "Turrets, vehicles, electricity - if it runs on volts or wheels, you built it. The apocalypse has a power grid now. Yours.",
                StatLines = new[] { new StatEntry("Electrician Skill 55/100", "ui_game_symbol_electric_generator"), new StatEntry("Vehicles Skill 20/100", "ui_game_symbol_4x4"), new StatEntry("Robotics 76/100", "ui_game_symbol_junk_turret"), new StatEntry("Tech Junkie 7/7", "ui_game_symbol_electric_turret"), new StatEntry("Workstations 34/75", "ui_game_symbol_workbench"), new StatEntry("Advanced Engineering 5/5", "ui_game_symbol_workbench"), new StatEntry("Grease Monkey 5/5", "ui_game_symbol_service") }
            },
            ["Ex-Soldier"] = new KitOverviewData
            {
                DisplayName = "Ex-Soldier",
                PreviewTitle = "Combat Veteran",
                Description = "Old rank, new rules. Keep moving, keep firing. Discipline is the last thing that survived.",
                StatLines = new[] { new StatEntry("Machine Gunner 5/5", "ui_game_symbol_rifle"), new StatEntry("Run and Gun 5/5", "ui_game_symbol_run_and_gun"), new StatEntry("Commando Armor Fullset"), new StatEntry("Armor Skill 11/100", "ui_game_symbol_armor_iron"), new StatEntry("Medium Armor 4/4", "ui_game_symbol_light_armor2"), new StatEntry("Urban Combat 7/7", "ui_game_symbol_light_armor2"), new StatEntry("Auto Weapon Handbook 7/7", "ui_game_symbol_rifle"), new StatEntry("Machine Guns 11/100", "ui_game_symbol_rifle") }
            },
            ["Doctor"] = new KitOverviewData
            {
                DisplayName = "Doctor",
                PreviewTitle = "Field Medic",
                Description = "Someone has to keep the idiots alive. Might as well be you.",
                StatLines = new[] { new StatEntry("Physician 5/5", "ui_game_symbol_medical"), new StatEntry("Charismatic Nature 3/5", "ui_game_symbol_talk"), new StatEntry("Medical 75/75", "ui_game_symbol_medical"), new StatEntry("Foot 27/100", "ui_game_symbol_armor_iron") }
            },
            ["Miner"] = new KitOverviewData
            {
                DisplayName = "Miner",
                PreviewTitle = "Resource Breaker",
                Description = "You hit rocks for a living. Zombies are just rocks that bleed.",
                StatLines = new[] { new StatEntry("Skull Crusher 5/5", "ui_game_symbol_sledge"), new StatEntry("Grand Slam 5/5", "ui_game_symbol_grand_slam"), new StatEntry("Strength Master 5/5", "ui_game_symbol_strength_mastery"), new StatEntry("Pack Mule 5/5", "ui_game_symbol_pack_mule"), new StatEntry("Miner 69'er 5/5", "ui_game_symbol_mining"), new StatEntry("Mother Lode 5/5", "ui_game_symbol_mother_load"), new StatEntry("Sledge Saga 7/7", "ui_game_symbol_sledge"), new StatEntry("Harvesting Tools 11/100", "ui_game_symbol_tool"), new StatEntry("Sledge Hammer 11/75", "ui_game_symbol_sledge"), new StatEntry("Workstations 34/75", "ui_game_symbol_workbench") }
            },
            ["Demoman"] = new KitOverviewData
            {
                DisplayName = "Demoman",
                PreviewTitle = "Explosive Expert",
                Description = "Collateral damage is just a fancy word for fun. If it's still standing, use a bigger one.",
                StatLines = new[] { new StatEntry("Demolition Expert 5/5", "ui_game_symbol_explosion"), new StatEntry("The Infiltrator 5/5", "ui_game_symbol_stealth"), new StatEntry("Explosives 50/100", "ui_game_symbol_explosion") }
            },
            ["Hitman"] = new KitOverviewData
            {
                DisplayName = "Hitman",
                PreviewTitle = "Silent Elimination",
                Description = "Clean. Quiet. Gone before they hit the floor. Getting paid in cans these days, but the craft stays the same.",
                StatLines = new[] { new StatEntry("Gunslinger 5/5", "ui_game_symbol_gunslinger"), new StatEntry("Run and Gun 5/5", "ui_game_symbol_run_and_gun"), new StatEntry("Agility Mastery 2/5", "ui_game_symbol_agility_mastery"), new StatEntry("Hidden Strike 5/5", "ui_game_symbol_sneak_attack"), new StatEntry("From the Shadows 5/5", "ui_game_symbol_bat"), new StatEntry("Pistol Pete 7/7", "ui_game_symbol_pistol"), new StatEntry("Magnum Enforcer 7/7", "ui_game_symbol_gunslinger"), new StatEntry("Handguns 26/100", "ui_game_symbol_pistol") }
            },
            ["Dumb Luck"] = new KitOverviewData
            {
                DisplayName = "Dumb Luck",
                PreviewTitle = "Risk Reward",
                Description = "You're not skilled. You're not smart. But somehow you always find the good stuff. Don't question it.",
                StatLines = new[] { new StatEntry("Lucky Looter (Skill) 5/5", "ui_game_symbol_shopping_cart"), new StatEntry("Lucky Looter Book 7/7", "ui_game_symbol_shopping_cart"), new StatEntry("Wasteland Treasures 7/7", "ui_game_symbol_treasure"), new StatEntry("Daring Adventurer 5/5", "ui_game_symbol_adventure"), new StatEntry("Treasure Hunter 5/5", "ui_game_symbol_treasure") }
            },
            ["Burglar"] = new KitOverviewData
            {
                DisplayName = "Burglar",
                PreviewTitle = "Infiltration",
                Description = "Why fight when you can just... take it? Locks open, shadows hide, blades finish the conversation.",
                StatLines = new[] { new StatEntry("Better Barter 5/5", "ui_game_symbol_barter"), new StatEntry("Lock Picking 3/3", "ui_game_symbol_unlock"), new StatEntry("Workstations 12/75", "ui_game_symbol_workbench"), new StatEntry("Blades 36/75", "ui_game_symbol_knife"), new StatEntry("Deep Cuts 5/5", "ui_game_symbol_deep_cuts"), new StatEntry("Hidden Strike 3/5", "ui_game_symbol_sneak_attack"), new StatEntry("From the Shadows 3/5", "ui_game_symbol_bat"), new StatEntry("Whirlwind 5/5", "ui_game_symbol_knunchuck") }
            }
        };

        private static readonly Dictionary<string, KitRewardData> KitRewards = new Dictionary<string, KitRewardData>(StringComparer.OrdinalIgnoreCase)
        {
            ["Scavenger"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkSalvageOperations"] = 5,
                    ["perkPerceptionMastery"] = 2,
                    ["craftingSalvageTools"] = 4
                }
            },
            ["Huntsman"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkDeadEye"] = 5,
                    ["perkAnimalTracker"] = 5,
                    ["perkPenetrator"] = 5,
                    ["perkTheHuntsman"] = 5,
                    ["perkIronGut"] = 5,
                    ["perkHiddenStrike"] = 2,
                    ["craftingRifles"] = 26,
                    ["perkSniperDamage"] = 1,
                    ["perkSniperCripplingShot"] = 1,
                    ["perkSniperHeadShot"] = 1,
                    ["perkSniperReload"] = 1,
                    ["perkSniperControlledBreathing"] = 1,
                    ["perkSniperAPAmmo"] = 1,
                    ["perkSniperHPAmmo"] = 1,
                    ["perkSniperComplete"] = 1
                }
            },
            ["Athlete"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkParkour"] = 5,
                    ["perkHardTarget"] = 5,
                    ["perkRunAndGun"] = 3,
                    ["craftingArmor"] = 11,
                    ["perkRuleOneCardio"] = 5
                }
            },
            ["Tyson"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkBrawler"] = 5,
                    ["perkFlurryOfFortitude"] = 5,
                    ["perkPainTolerance"] = 5,
                    ["perkFortitudeMastery"] = 5,
                    ["perkSiphoningStrikes"] = 5,
                    ["perkHealingFactor"] = 5,
                    ["perkIronGut"] = 5,
                    ["craftingKnuckles"] = 11,
                    ["perkBarBrawling1BasicMoves"] = 1,
                    ["perkBarBrawling2DropABomb"] = 1,
                    ["perkBarBrawling3KillerInstinct"] = 1,
                    ["perkBarBrawling4FinishingMoves"] = 1,
                    ["perkBarBrawling5AdrenalineHealing"] = 1,
                    ["perkBarBrawling6RageMode"] = 1,
                    ["perkBarBrawling7BoozedUp"] = 1,
                    ["perkBarBrawling8Complete"] = 1
                }
            },
            ["Archer"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkArchery"] = 5,
                    ["perkFromTheShadows"] = 2,
                    ["perkHiddenStrike"] = 3,
                    ["craftingBows"] = 11,
                    ["perkRangersArrowRecovery"] = 1,
                    ["perkRangersExplodingBolts"] = 1,
                    ["perkRangersCripplingShot"] = 1,
                    ["perkRangersAPAmmo"] = 1,
                    ["perkRangersFlamingArrows"] = 1,
                    ["perkRangersForestGuide"] = 1,
                    ["perkRangersKnockdown"] = 1,
                    ["perkRangersComplete"] = 1
                }
            },
            ["Farmer"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkLivingOffTheLand"] = 3,
                    ["craftingArmor"] = 11,
                    ["perkMediumArmor"] = 4,
                    ["craftingSeeds"] = 20,
                    ["craftingFood"] = 100,
                    ["perkJavelinMaster"] = 5,
                    ["perkFlurryOfPerception"] = 5,
                    ["craftingSpears"] = 11,
                    ["perkSpearHunter1Damage"] = 1,
                    ["perkSpearHunter2Maintenance"] = 1,
                    ["perkSpearHunter3Bleed"] = 1,
                    ["perkSpearHunter4KillMove"] = 1,
                    ["perkSpearHunter5RapidStrike"] = 1,
                    ["perkSpearHunter6PenetratingShaft"] = 1,
                    ["perkSpearHunter7QuickStrike"] = 1,
                    ["perkSpearHunter8Complete"] = 1
                },
                CustomVars = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    ["plantedGraceCorn1"] = 1f
                },
                ItemRewards = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["armorFarmerHelmet"] = 1,
                    ["armorFarmerOutfit"] = 1,
                    ["armorFarmerGloves"] = 1,
                    ["armorFarmerBoots"] = 1
                }
            },
            ["Engineer"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["craftingElectrician"] = 55,
                    ["craftingVehicles"] = 20,
                    ["craftingRobotics"] = 76,
                    ["craftingWorkstations"] = 34,
                    ["perkAdvancedEngineering"] = 5,
                    ["perkGreaseMonkey"] = 5,
                    ["perkTechJunkie1Damage"] = 1,
                    ["perkTechJunkie2Maintenance"] = 1,
                    ["perkTechJunkie3APAmmo"] = 1,
                    ["perkTechJunkie4Shells"] = 1,
                    ["perkTechJunkie5Repulsor"] = 1,
                    ["perkTechJunkie6BatonCharge"] = 1,
                    ["perkTechJunkie7Hydraulics"] = 1,
                    ["perkTechJunkie8Complete"] = 1
                }
            },
            ["Ex-Soldier"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkMachineGunner"] = 5,
                    ["perkRunAndGun"] = 5,
                    ["craftingArmor"] = 11,
                    ["perkMediumArmor"] = 4,
                    ["craftingMachineGuns"] = 11,
                    ["perkUrbanCombatLanding"] = 1,
                    ["perkUrbanCombatCigar"] = 1,
                    ["perkUrbanCombatSneaking"] = 1,
                    ["perkUrbanCombatJumping"] = 1,
                    ["perkUrbanCombatLandMines"] = 1,
                    ["perkUrbanCombatAdrenalineRush"] = 1,
                    ["perkUrbanCombatRoomClearing"] = 1,
                    ["perkUrbanCombatComplete"] = 1,
                    ["perkAutoWeaponsDamage"] = 1,
                    ["perkAutoWeaponsUncontrolledBurst"] = 1,
                    ["perkAutoWeaponsMaintenance"] = 1,
                    ["perkAutoWeaponsDrumMag"] = 1,
                    ["perkAutoWeaponsRecoil"] = 1,
                    ["perkAutoWeaponsRagdoll"] = 1,
                    ["perkAutoWeaponsMachineGuns"] = 1,
                    ["perkAutoWeaponsComplete"] = 1
                },
                ItemRewards = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["armorCommandoHelmet"] = 1,
                    ["armorCommandoOutfit"] = 1,
                    ["armorCommandoGloves"] = 1,
                    ["armorCommandoBoots"] = 1
                }
            },
            ["Doctor"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkPhysician"] = 5,
                    ["perkCharismaticNature"] = 3,
                    ["craftingMedical"] = 75,
                    ["craftingArmor"] = 27
                }
            },
            ["Miner"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkSkullCrusher"] = 5,
                    ["perkGrandSlam"] = 5,
                    ["perkStrengthMastery"] = 5,
                    ["perkPackMule"] = 5,
                    ["perkMiner69r"] = 5,
                    ["perkMotherLode"] = 5,
                    ["craftingHarvestingTools"] = 11,
                    ["craftingSledgehammers"] = 11,
                    ["craftingWorkstations"] = 34,
                    ["perkSledgeSagaKnockdown"] = 1,
                    ["perkSledgeSagaDegradation"] = 1,
                    ["perkSledgeSagaCrippledMorale"] = 1,
                    ["perkSledgeSagaPulverizingFinishers"] = 1,
                    ["perkSledgeSagaSavageReaper"] = 1,
                    ["perkSledgeSagaConcussiveStrike"] = 1,
                    ["perkSledgeSagaArmorCrusher"] = 1,
                    ["perkSledgeSagaComplete"] = 1
                }
            },
            ["Demoman"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkDemolitionsExpert"] = 5,
                    ["perkInfiltrator"] = 5,
                    ["craftingExplosives"] = 50
                }
            },
            ["Hitman"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkGunslinger"] = 5,
                    ["perkRunAndGun"] = 5,
                    ["perkAgilityMastery"] = 2,
                    ["perkHiddenStrike"] = 5,
                    ["perkFromTheShadows"] = 5,
                    ["craftingHandguns"] = 26,
                    ["perkPistolPeteTakeAim"] = 1,
                    ["perkPistolPeteSwissKnees"] = 1,
                    ["perkPistolPeteSteadyHand"] = 1,
                    ["perkPistolPeteMaintenance"] = 1,
                    ["perkPistolPeteHPAmmo"] = 1,
                    ["perkPistolPeteAPAmmo"] = 1,
                    ["perkPistolPeteDamage"] = 1,
                    ["perkPistolPeteComplete"] = 1,
                    ["perkEnforcerDamage"] = 1,
                    ["perkEnforcerApparel"] = 1,
                    ["perkEnforcerPunks"] = 1,
                    ["perkEnforcerIntimidation"] = 1,
                    ["perkEnforcerAPAmmo"] = 1,
                    ["perkEnforcerHPAmmo"] = 1,
                    ["perkEnforcerCriminalPursuit"] = 1,
                    ["perkEnforcerComplete"] = 1
                }
            },
            ["Dumb Luck"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkLuckyLooter"] = 5,
                    ["perkLuckyLooterDukes"] = 1,
                    ["perkLuckyLooterAmmunition"] = 1,
                    ["perkLuckyLooterBrass"] = 1,
                    ["perkLuckyLooterLead"] = 1,
                    ["perkLuckyLooterBooks"] = 1,
                    ["perkLuckyLooterFood"] = 1,
                    ["perkLuckyLooterMedical"] = 1,
                    ["perkLuckyLooterComplete"] = 1,
                    ["perkWasteTreasuresHoney"] = 1,
                    ["perkWasteTreasuresCoffins"] = 1,
                    ["perkWasteTreasuresAcid"] = 1,
                    ["perkWasteTreasuresWater"] = 1,
                    ["perkWasteTreasuresDoors"] = 1,
                    ["perkWasteTreasuresCloth"] = 1,
                    ["perkWasteTreasuresSinks"] = 1,
                    ["perkWasteTreasuresComplete"] = 1,
                    ["perkDaringAdventurer"] = 5,
                    ["perkTreasureHunter"] = 5
                }
            },
            ["Burglar"] = new KitRewardData
            {
                ProgressionFloors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["perkBetterBarter"] = 5,
                    ["perkLockPicking"] = 3,
                    ["craftingWorkstations"] = 12,
                    ["craftingBlades"] = 36,
                    ["perkDeepCuts"] = 5,
                    ["perkFlurryOfAgility"] = 5,
                    ["perkHiddenStrike"] = 3,
                    ["perkFromTheShadows"] = 3
                }
            }
        };

        private const string FloorEnabledVar = "skFloorEnabled";
        private const string FloorVarPrefix = "skFloor_";
        private static readonly Color32 LockedKitBorderColor = new Color32(220, 64, 64, 255);

        private static readonly string[] CandidateTextPropertyNames = { "Text", "Caption" };
        private static readonly string[] CandidateTextKeyPropertyNames = { "TextKey", "CaptionKey" };

        private readonly List<KitButtonEntry> kitButtons = new List<KitButtonEntry>();
        private readonly List<KitPreviewImageEntry> kitPreviewImages = new List<KitPreviewImageEntry>();
        private string selectedKitName;
        private string playerLockedKitName;
        private XUiController selectedKitNameLabel;
        private XUiController lockedKitLabel;
        private XUiController overviewHintLabel;
        private XUiController previewLabel;
        private XUiController previewHintLabel;
        private XUiController attributesLabel;
        private const int MaxStatRows = 12;
        private readonly XUiController[] statIconControllers = new XUiController[MaxStatRows];
        private readonly XUiController[] statLabelControllers = new XUiController[MaxStatRows];
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

            this.RegisterKitTexture("Scavenger", "texScavenger");
            this.RegisterKitTexture("Huntsman", "texHuntsman");
            this.RegisterKitTexture("Athlete", "texAthlete");
            this.RegisterKitTexture("Tyson", "texTyson");
            this.RegisterKitTexture("Archer", "texArcher");
            this.RegisterKitTexture("Farmer", "texFarmer");
            this.RegisterKitTexture("Engineer", "texEngineer");
            this.RegisterKitTexture("Ex-Soldier", "texExSoldier");
            this.RegisterKitTexture("Doctor", "texDoctor");
            this.RegisterKitTexture("Miner", "texMiner");
            this.RegisterKitTexture("Demoman", "texDemoman");
            this.RegisterKitTexture("Hitman", "texHitman");
            this.RegisterKitTexture("Dumb Luck", "texDumbLuck");
            this.RegisterKitTexture("Burglar", "texBurglar");

            this.selectedKitNameLabel = this.GetChildById("lblSelectedKitName");
            this.lockedKitLabel = this.GetChildById("lblLockedKit");
            this.overviewHintLabel = this.GetChildById("lblOverviewHint");
            this.previewLabel = this.GetChildById("lblKitImagePlaceholder");
            this.previewHintLabel = this.GetChildById("lblKitImageHint");
            this.attributesLabel = this.GetChildById("lblKitAttributes");
            for (int i = 0; i < MaxStatRows; i++)
            {
                this.statIconControllers[i] = this.GetChildById("statIcon" + i);
                this.statLabelControllers[i] = this.GetChildById("statLabel" + i);
            }
            this.confirmButtonController = this.GetChildById("btnConfirmKit");
            this.confirmButton = this.confirmButtonController as XUiC_SimpleButton;
            if (this.confirmButton == null)
            {
                this.confirmButton = this.confirmButtonController?.GetChildByType<XUiC_SimpleButton>();
            }

            if (this.confirmButton != null)
            {
                this.confirmButton.OnPressed += this.OnConfirmButtonPressed;
                Log.Out("[StarterKits] Confirm button OnPressed handler attached.");
            }
            else
            {
                Log.Warning("[StarterKits] Confirm button could not be resolved; confirm will not trigger rewards.");
            }

            Log.Out($"[StarterKits] Init complete. Buttons={this.kitButtons.Count}, previews={this.kitPreviewImages.Count}");

            this.LoadPlayerLockedKit();
            this.RefreshUI();
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.LoadPlayerLockedKit();
            this.RefreshUI();
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

        public void eventConfirmKit(XUiController sender, string _onClick)
        {
            Log.Out("[StarterKits] eventConfirmKit invoked from XML event_onclick.");
            this.OnConfirmButtonPressed(sender, -1);
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

        private void RegisterKitTexture(string kitName, string textureId)
        {
            var textureController = this.GetChildById(textureId);
            if (textureController?.ViewComponent == null)
            {
                Log.Warning($"[StarterKits] Failed to register kit texture '{kitName}'. texture={textureId}");
                return;
            }

            Log.Out($"[StarterKits] Registered kit preview '{kitName}' with controller '{textureId}' ({textureController.ViewComponent.GetType().Name}).");
            this.kitPreviewImages.Add(new KitPreviewImageEntry
            {
                KitName = kitName,
                TextureController = textureController
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
                        var lockedKit = StarterKitSelectionStore.GetSelected(player);
                        this.playerLockedKitName = lockedKit;
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

        private void RefreshUI()
        {
            if (string.IsNullOrEmpty(this.playerLockedKitName))
            {
                this.selectedKitName = null;
                this.ClearSelection();
            }
            else
            {
                // Player already locked a kit; show it with no further selection allowed.
                this.selectedKitName = this.playerLockedKitName;
                this.UpdateLockedKitDisplay();
            }
        }

        private void SelectKit(string kitName)
        {
            if (!string.IsNullOrEmpty(this.playerLockedKitName))
            {
                // Already locked: allow browsing all kits, but keep confirm blocked.
                this.selectedKitName = kitName;
                this.UpdateLockedKitDisplay();
                return;
            }

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

        private void UpdateLockedKitDisplay()
        {
            this.UpdateLockedKitSelectionVisual();
            this.UpdateLockedKitLabel();

            string overviewKit = string.IsNullOrEmpty(this.selectedKitName)
                ? this.playerLockedKitName
                : this.selectedKitName;
            this.UpdateOverview(overviewKit);

            if (!string.IsNullOrEmpty(this.playerLockedKitName))
            {
                this.SetText(this.overviewHintLabel,
                    $"Kit lock active: {this.playerLockedKitName}. You can preview all kits, but cannot confirm a new one.");
            }
        }

        private void UpdateLockedKitSelectionVisual()
        {
            for (int i = 0; i < this.kitButtons.Count; i++)
            {
                bool isLocked = string.Equals(this.kitButtons[i].KitName, this.playerLockedKitName, StringComparison.OrdinalIgnoreCase);
                bool isSelected = string.Equals(this.kitButtons[i].KitName, this.selectedKitName, StringComparison.OrdinalIgnoreCase);
                if (this.kitButtons[i].SelectedOverlay?.ViewComponent != null)
                {
                    this.kitButtons[i].SelectedOverlay.ViewComponent.IsVisible = isLocked || isSelected;
                    if (isLocked)
                    {
                        this.TrySetLockedBorderColor(this.kitButtons[i].SelectedOverlay);
                    }
                }
            }
        }

        private void TrySetLockedBorderColor(XUiController overlayController)
        {
            if (overlayController == null || overlayController.Children == null || overlayController.Children.Count == 0)
            {
                return;
            }

            // Overlay structure in XML: child[0]=fill sprite, child[1]=border sprite.
            for (int i = 0; i < overlayController.Children.Count; i++)
            {
                bool isBorderSprite = i == 1;
                if (isBorderSprite)
                {
                    this.TrySetViewColor(overlayController.Children[i].ViewComponent, LockedKitBorderColor);
                }
            }
        }

        private void TrySetViewColor(object viewComponent, Color32 color)
        {
            if (viewComponent == null)
            {
                return;
            }

            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type viewType = viewComponent.GetType();

            PropertyInfo colorProp = viewType.GetProperty("Color", Flags);
            if (colorProp != null && colorProp.CanWrite)
            {
                if (colorProp.PropertyType == typeof(Color))
                {
                    colorProp.SetValue(viewComponent, (Color)color, null);
                    return;
                }

                if (colorProp.PropertyType == typeof(Color32))
                {
                    colorProp.SetValue(viewComponent, color, null);
                    return;
                }
            }

            FieldInfo colorField = viewType.GetField("Color", Flags);
            if (colorField != null)
            {
                if (colorField.FieldType == typeof(Color))
                {
                    colorField.SetValue(viewComponent, (Color)color);
                    return;
                }

                if (colorField.FieldType == typeof(Color32))
                {
                    colorField.SetValue(viewComponent, color);
                }
            }
        }

        private void UpdateLockedKitLabel()
        {
            if (this.lockedKitLabel?.ViewComponent == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.playerLockedKitName))
            {
                this.lockedKitLabel.ViewComponent.IsVisible = false;
                return;
            }

            this.lockedKitLabel.ViewComponent.IsVisible = true;
            
            try
            {
                Type viewType = this.lockedKitLabel.ViewComponent?.GetType();
                if (viewType != null)
                {
                    var textProp = viewType.GetProperty("Text", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (textProp != null && textProp.CanWrite)
                    {
                        textProp.SetValue(this.lockedKitLabel.ViewComponent, $"Your Kit: {this.playerLockedKitName}", null);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] Could not set locked kit label text: {ex.Message}");
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

        private void ApplySelectedKitRewards(EntityPlayer player, string kitName)
        {
            if (player == null || string.IsNullOrEmpty(kitName))
            {
                return;
            }

            if (!KitRewards.TryGetValue(kitName, out KitRewardData reward) || reward?.ProgressionFloors == null)
            {
                Log.Warning($"[StarterKits] No reward definition found for kit '{kitName}'.");
                return;
            }

            this.ApplyKitFloors(player, kitName, reward);
            this.ApplyKitItems(player, kitName, reward.ItemRewards);
        }

        private void ApplyKitFloors(EntityPlayer player, string kitName, KitRewardData reward)
        {
            if (player?.Buffs == null)
            {
                return;
            }

            // During development one-time lock is disabled, so clear previous kit floors first.
            this.ClearAllFloorVars(player);

            int applied = 0;
            try
            {
                this.SetBuffCustomVar(player, FloorEnabledVar, 1f);

                if (reward?.ProgressionFloors != null)
                {
                    foreach (var kvp in reward.ProgressionFloors)
                    {
                        if (string.IsNullOrEmpty(kvp.Key) || kvp.Value <= 0)
                        {
                            continue;
                        }

                        string varName = FloorVarPrefix + kvp.Key;
                        this.SetBuffCustomVar(player, varName, kvp.Value);
                        applied++;
                    }
                }

                if (reward?.CustomVars != null)
                {
                    foreach (var kvp in reward.CustomVars)
                    {
                        if (string.IsNullOrEmpty(kvp.Key))
                        {
                            continue;
                        }

                        this.SetBuffCustomVar(player, kvp.Key, kvp.Value);
                    }
                }

                Log.Out($"[StarterKits] Applied kit floors for '{kitName}': {applied} progression floors.");
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] Failed to apply kit floors for '{kitName}': {ex.Message}");
            }
        }

        private void ClearAllFloorVars(EntityPlayer player)
        {
            if (player?.Buffs == null)
            {
                return;
            }

            this.SetBuffCustomVar(player, FloorEnabledVar, 0f);

            foreach (var reward in KitRewards.Values)
            {
                if (reward?.ProgressionFloors == null)
                {
                    continue;
                }

                foreach (var progressionName in reward.ProgressionFloors.Keys)
                {
                    if (string.IsNullOrEmpty(progressionName))
                    {
                        continue;
                    }

                    this.SetBuffCustomVar(player, FloorVarPrefix + progressionName, 0f);
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

        private void ApplyKitItems(EntityPlayer player, string kitName, Dictionary<string, int> itemRewards)
        {
            if (player == null || itemRewards == null || itemRewards.Count == 0)
            {
                return;
            }

            var overflow = new List<ItemStack>();
            int delivered = 0;

            foreach (var kvp in itemRewards)
            {
                if (string.IsNullOrEmpty(kvp.Key) || kvp.Value <= 0)
                {
                    continue;
                }

                ItemValue itemValue = ItemClass.GetItem(kvp.Key, true);
                if (itemValue == null || itemValue.IsEmpty())
                {
                    Log.Warning($"[StarterKits] Item reward skipped for '{kitName}': unknown item '{kvp.Key}'.");
                    continue;
                }

                ushort quality = this.GetArmorRewardQuality(kitName, kvp.Key);
                if (quality > 0)
                {
                    itemValue.Quality = quality;
                }

                var stack = new ItemStack(itemValue, kvp.Value);
                
                if (this.TryAddItemToInventory(player, stack))
                {
                    delivered += kvp.Value;
                    Log.Out($"[StarterKits] Item '{kvp.Key}' (qty={kvp.Value}) added to inventory for kit '{kitName}'.");
                }
                else
                {
                    overflow.Add(stack);
                    Log.Out($"[StarterKits] Item '{kvp.Key}' (qty={kvp.Value}) could not fit in inventory, will drop as loot.");
                }
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

                    Log.Out($"[StarterKits] Kit '{kitName}' item overflow dropped as loot bag(s): {overflow.Count} stack(s).");
                }
                catch (Exception ex)
                {
                    Log.Warning($"[StarterKits] Failed to drop overflow loot bag for '{kitName}': {ex.Message}");
                }
            }

            Log.Out($"[StarterKits] Kit '{kitName}' completed: {delivered} items to inventory, {overflow.Count} stacks to loot bag.");
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

        private ushort GetArmorRewardQuality(string kitName, string itemName)
        {
            if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(kitName))
            {
                return 0;
            }

            // Farmer and Ex-Soldier armor sets are granted as level 1 quality set pieces.
            if (string.Equals(kitName, "Farmer", StringComparison.OrdinalIgnoreCase) && itemName.StartsWith("armorFarmer", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            if (string.Equals(kitName, "Ex-Soldier", StringComparison.OrdinalIgnoreCase) && itemName.StartsWith("armorCommando", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            return 0;
        }

        private void UpdateOverview(string kitName)
        {
            if (string.IsNullOrEmpty(kitName) || !KitOverview.TryGetValue(kitName, out KitOverviewData data))
            {
                this.SetText(this.selectedKitNameLabel, "No Kit Selected");
                this.SetText(this.overviewHintLabel, "Pick a starter kit on the left to preview its description and perk stats.");
                this.SetText(this.previewLabel, "Select A Kit");
                this.SetText(this.attributesLabel, "Select a kit to inspect its description.");
                this.ApplyStatRows(null);
                this.SetVisible(this.previewLabel, true);
                this.SetVisible(this.previewHintLabel, true);
                this.UpdatePreviewTexture(null);
                return;
            }

            this.SetText(this.selectedKitNameLabel, data.DisplayName);
            this.SetText(this.overviewHintLabel, "Review the preview, then lock in the kit when you're ready.");
            this.SetText(this.previewLabel, data.PreviewTitle);
            this.SetText(this.attributesLabel, data.Description);
            this.ApplyStatRows(data.StatLines);
            this.SetVisible(this.previewLabel, false);
            this.SetVisible(this.previewHintLabel, false);
            this.UpdatePreviewTexture(kitName);
        }

        private void UpdatePreviewTexture(string selectedKit)
        {
            int visibleCount = 0;
            for (int i = 0; i < this.kitPreviewImages.Count; i++)
            {
                bool visible = !string.IsNullOrEmpty(selectedKit) &&
                    string.Equals(this.kitPreviewImages[i].KitName, selectedKit, StringComparison.OrdinalIgnoreCase);
                this.SetVisible(this.kitPreviewImages[i].TextureController, visible);
                if (visible)
                {
                    visibleCount++;
                }
            }

            if (!string.IsNullOrEmpty(selectedKit))
            {
                Log.Out($"[StarterKits] Preview selected={selectedKit}, visible={visibleCount}/{this.kitPreviewImages.Count}");
            }
        }

        private void ApplyStatRows(StatEntry[] entries)
        {
            int count = entries?.Length ?? 0;

            for (int i = 0; i < MaxStatRows; i++)
            {
                XUiController labelController = this.statLabelControllers[i];
                XUiController iconController = this.statIconControllers[i];

                if (i >= count)
                {
                    this.SetText(labelController, string.Empty);
                    this.SetVisible(iconController, false);
                    continue;
                }

                StatEntry entry = entries[i];
                this.SetText(labelController, entry.Text);

                if (!string.IsNullOrEmpty(entry.IconSpriteName) && iconController?.ViewComponent is XUiV_Sprite spriteView)
                {
                    spriteView.SpriteName = entry.IconSpriteName;
                    this.SetVisible(iconController, true);
                }
                else
                {
                    this.SetVisible(iconController, false);
                }
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