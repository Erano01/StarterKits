using HarmonyLib;
using UnityEngine;
using StarterKits;

namespace StarterKits.Harmony
{
    /// <summary>
    /// InGameMenu'nun Init metodunu patch'ler ve Starter Kit butonu için event handler ekler.
    /// </summary>
    [HarmonyPatch(typeof(XUiC_InGameMenuWindow))]
    [HarmonyPatch("Init")]
    public class InGameMenuPatch
    {
        private static void Postfix(XUiC_InGameMenuWindow __instance)
        {
            try
            {
                // Starter Kit butonunu bul
                var starterKitBtn = __instance.GetChildById("btnStarterKit")?.GetChildByType<XUiC_SimpleButton>();
                if (starterKitBtn != null)
                {
                    // Event handler ekle
                    starterKitBtn.OnPressed += OnStarterKitPressed;
                    
                    Log.Out("[StarterKits] Starter Kit button event handler attached successfully.");
                }
                else
                {
                    Log.Out("[StarterKits] Starter Kit button not found in InGame menu.");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[StarterKits] Error in InGameMenuPatch.Postfix: {ex}");
            }
        }

        public static void OnStarterKitPressed(XUiController _sender, int _mouseButton)
        {
            try
            {
                Log.Out("[StarterKits] Starter Kit button pressed.");
                
                // InGame menüsünü kapat
                var playerUI = _sender.xui?.playerUI;
                if (playerUI?.windowManager != null)
                {
                    playerUI.windowManager.Close(XUiC_InGameMenuWindow.ID);
                }
                
                // Starter kit menüsünü aç
                XUiC_KitSelectionMenu.OpenStarterKitMenu();
            }
            catch (System.Exception ex)
            {
                Log.Error($"[StarterKits] Error in OnStarterKitPressed: {ex}");
            }
        }
    }
}