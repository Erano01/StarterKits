using System;
using System.Reflection;

namespace StarterKits
{
    public static class PlayerJoinedGameHandler
    {
        // Static constructor
        static PlayerJoinedGameHandler()
        {
            Log.Out("[StarterKits] PlayerJoinedGameHandler static ctor running.");
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(OnPlayerSpawnedInWorld);
            ModEvents.GameStartDone.RegisterHandler(OnGameStartDone);
        }

        private static void OnGameStartDone(ref ModEvents.SGameStartDoneData _data)
        {
            Log.Out("[StarterKits] GameStartDone fired. Resolving world/save context.");
            StarterKitSelectionStore.OnGameStartDone();
        }

        private static void OnPlayerSpawnedInWorld(ref ModEvents.SPlayerSpawnedInWorldData _data)
        {
            // Re-check context in case GameStartDone fired before the store was initialized.
            StarterKitSelectionStore.OnGameStartDone();

            bool isNewGame = IsNewGameSpawn(ref _data);
            Log.Out($"[StarterKits] PlayerSpawnedInWorld fired. isNewGame={isNewGame}");

            if (isNewGame)
            {
                StarterKitSelectionStore.OnNewGameDetected();
            }

            XUiC_KitSelectionMenu.TryOpenForPlayer();
        }

        private static bool IsNewGameSpawn(ref ModEvents.SPlayerSpawnedInWorldData _data)
        {
            try
            {
                // Read respawn type via reflection and decide only from known values.
                object boxed = _data;
                Type dataType = boxed.GetType();

                // Try known field names for the respawn type.
                string[] candidates = { "_respawnType", "clientRespawnType", "respawnType", "RespawnType" };
                foreach (var name in candidates)
                {
                    FieldInfo f = dataType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (f == null) continue;

                    object val = f.GetValue(boxed);
                    if (val == null) continue;

                    string valStr = val.ToString();
                    Log.Out($"[StarterKits] Spawn respawn type field '{name}' = '{valStr}'");

                    // Explicitly confirmed from runtime logs: NewGame means a fresh save/session.
                    if (string.Equals(valStr, "NewGame", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    // Loaded/reconnect/death/teleport should not reset DB.
                    if (string.Equals(valStr, "EnterMultiplayer", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(valStr, "LoadedGame", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(valStr, "JoinMultiplayer", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(valStr, "Teleport", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(valStr, "Dead", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(valStr, "Respawn", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    // Unknown values are treated as non-new to avoid accidental DB wipes.
                    Log.Out($"[StarterKits] Unknown respawn type '{valStr}', treating as existing save.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] IsNewGameSpawn reflection failed: {ex.Message}");
            }

            // Could not determine → safe default: do NOT reset (avoid accidental wipes).
            return false;
        }

        // for triggering static constructor.
        public static void Init()
        {
        }
    }
}

