using System;

namespace StarterKits
{
    public static class PlayerJoinedGameHandler
    {
        // Static constructor
        static PlayerJoinedGameHandler()
        {
            Log.Out("[StarterKits] PlayerJoinedGameHandler static ctor running.");
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(OnPlayerSpawnedInWorld);
        }

        private static void OnPlayerSpawnedInWorld(ref ModEvents.SPlayerSpawnedInWorldData _data)
        {
            Log.Out("[StarterKits] PlayerSpawnedInWorld fired, trying to open starter kit window.");
            XUiC_KitSelectionMenu.TryOpenForPlayer();
        }

        // for triggering static constructor.
        public static void Init()
        {
        }
    }
}

