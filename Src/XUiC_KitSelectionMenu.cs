using System;
using System.Collections.Generic;

/// <summary>
/// Starter kit seçim menüsü controller'ı.
/// XML'deki event_onclick="eventChooseKit" ile çalışır.
/// </summary>
namespace StarterKits
{
    public class XUiC_KitSelectionMenu : XUiController
    {
        private sealed class KitButtonEntry
        {
            public string KitName;
            public XUiC_SimpleButton Button;
            public XUiController SelectedOverlay;
        }

        private readonly List<KitButtonEntry> kitButtons = new List<KitButtonEntry>();
        private string selectedKitName;

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