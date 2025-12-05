using System.Reflection;

namespace Harmony
{
    public class Application : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            var harmony = new HarmonyLib.Harmony(_modInstance.Name);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            StarterKits.PlayerJoinedGameHandler.Init();
            // XUi controller tipimizin assembly'yi yüklerken canlı olduğundan emin olalım
            var t = typeof(StarterKits.XUiC_KitSelectionMenu);
            Log.Out($"[StarterKits] XUiC_KitSelectionMenu type loaded: {t.Assembly.FullName}");
        }
    }
}
