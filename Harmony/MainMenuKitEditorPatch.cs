using HarmonyLib;
using StarterKits.Editor;

namespace StarterKits.Harmony
{
    /// <summary>
    /// Hooks the "Starter Kits" main menu button (Config/XUi_Menu/windows.xml) to open the kit editor.
    /// XUiC_MainMenuButtons only wires its own hard-coded button names, so ours needs a postfix.
    /// </summary>
    [HarmonyPatch(typeof(XUiC_MainMenuButtons))]
    [HarmonyPatch("Init")]
    public class MainMenuKitEditorPatch
    {
        private const string ButtonId = "btnStarterKitEditor";

        private static void Postfix(XUiC_MainMenuButtons __instance)
        {
            // XUiC_MainMenuButtons is also the controller of the version/news windows; only the main grid has our button.
            if (!__instance.TryGetChildByIdAndType<XUiC_Button>(ButtonId, out XUiC_Button button))
            {
                return;
            }

            button.OnPress += (_sender, _mouseButton) =>
            {
                XUi xui = __instance.xui;
                xui.playerUI.windowManager.Close(__instance.WindowGroup.Id);
                xui.playerUI.windowManager.Open(XUiC_KitEditorWindow.ID, _bModal: true);
            };

            Log.Out("[StarterKits] Main menu 'Starter Kits' editor button attached.");
        }
    }
}
