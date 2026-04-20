using System.Reflection;

namespace StarterKits
{
    public class StarterKitsModApi : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            var harmony = new HarmonyLib.Harmony(_modInstance.Name);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Harmony.StarterKitProgressionFloorPatch.Register(harmony);
            Log.Out($"[StarterKits] Harmony patches applied from assembly: {Assembly.GetExecutingAssembly().FullName}");

            StarterKitSelectionStore.InitializeOnModLoad();

            PlayerJoinedGameHandler.Init();
            // XUi controller tipimizin assembly'yi yüklerken canlı olduğundan emin olalım
            var t = typeof(XUiC_KitSelectionMenu);
            Log.Out($"[StarterKits] XUiC_KitSelectionMenu type loaded: {t.Assembly.FullName}");
        }
    }
}
