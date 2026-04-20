using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StarterKits
{
    /// <summary>
    /// Persists one-time starter kit selection on disk so reconnects cannot reopen selection.
    /// </summary>
    public static class StarterKitSelectionStore
    {
        private static readonly object Sync = new object();
        private static readonly HashSet<string> SelectedPlayers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static string loadedFromPath;

        private static string FilePath
        {
            get
            {
                // Primary: use the game's save root dir so data is co-located with the active world save.
                // GameIO.m_CachedSaveGameRootDir = e.g. /home/user/.local/share/7DaysToDie/Saves/WorldName/SaveName
                string saveRoot = null;
                try
                {
                    var gameIOType = Type.GetType("GameIO") ?? typeof(GameManager).Assembly.GetType("GameIO");
                    if (gameIOType != null)
                    {
                        var field = gameIOType.GetField("m_CachedSaveGameRootDir",
                            BindingFlags.Public | BindingFlags.Static);
                        if (field != null)
                        {
                            saveRoot = field.GetValue(null) as string;
                        }
                    }
                }
                catch
                {
                    // Ignore; fall back to Mods folder.
                }

                if (!string.IsNullOrWhiteSpace(saveRoot))
                {
                    return Path.Combine(saveRoot, "StarterKits", "selected-players.txt");
                }

                // Fallback: Mods/StarterKits/Data/selected-players.txt
                return Path.Combine(Environment.CurrentDirectory, "Mods", "StarterKits", "Data", "selected-players.txt");
            }
        }

        public static bool HasSelected(EntityPlayer player)
        {
            string stableId = GetStablePlayerId(player);
            if (string.IsNullOrEmpty(stableId))
            {
                return false;
            }

            EnsureLoaded();
            lock (Sync)
            {
                return SelectedPlayers.Contains(stableId);
            }
        }

        public static string GetSelected(EntityPlayer player)
        {
            string stableId = GetStablePlayerId(player);
            if (string.IsNullOrEmpty(stableId))
            {
                return null;
            }

            EnsureLoaded();

            lock (Sync)
            {
                try
                {
                    if (File.Exists(FilePath))
                    {
                        foreach (var line in File.ReadAllLines(FilePath))
                        {
                            if (string.IsNullOrWhiteSpace(line))
                            {
                                continue;
                            }

                            int separator = line.IndexOf('|');
                            string id = separator >= 0 ? line.Substring(0, separator) : line;
                            if (string.Equals(id.Trim(), stableId, StringComparison.Ordinal))
                            {
                                int kitSeparator = line.IndexOf('|', separator + 1);
                                if (kitSeparator > separator + 1)
                                {
                                    string kitName = line.Substring(separator + 1, kitSeparator - separator - 1).Trim();
                                    if (!string.IsNullOrEmpty(kitName))
                                    {
                                        return kitName;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"[StarterKits] Could not read selected kit for player: {ex.Message}");
                }
            }

            return null;
        }

        public static void MarkSelected(EntityPlayer player, string kitName)
        {
            string stableId = GetStablePlayerId(player);
            if (string.IsNullOrEmpty(stableId))
            {
                Log.Warning("[StarterKits] Could not resolve stable player id; persistent one-time lock not written.");
                return;
            }

            EnsureLoaded();

            lock (Sync)
            {
                if (!SelectedPlayers.Add(stableId))
                {
                    return;
                }

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    using (var writer = File.AppendText(FilePath))
                    {
                        writer.WriteLine(stableId + "|" + (kitName ?? string.Empty) + "|" + DateTime.UtcNow.ToString("o"));
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"[StarterKits] Could not write selection store: {ex.Message}");
                }
            }
        }

        private static void EnsureLoaded()
        {
            string currentPath = FilePath;
            if (string.Equals(loadedFromPath, currentPath, StringComparison.Ordinal))
            {
                return;
            }

            lock (Sync)
            {
                if (string.Equals(loadedFromPath, currentPath, StringComparison.Ordinal))
                {
                    return;
                }

                SelectedPlayers.Clear();

                try
                {
                    if (File.Exists(currentPath))
                    {
                        foreach (var line in File.ReadAllLines(currentPath))
                        {
                            if (string.IsNullOrWhiteSpace(line))
                            {
                                continue;
                            }

                            int separator = line.IndexOf('|');
                            string id = separator >= 0 ? line.Substring(0, separator) : line;
                            if (!string.IsNullOrWhiteSpace(id))
                            {
                                SelectedPlayers.Add(id.Trim());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"[StarterKits] Could not load selection store: {ex.Message}");
                }

                loadedFromPath = currentPath;
            }
        }

        private static string GetStablePlayerId(EntityPlayer player)
        {
            if (player == null)
            {
                return null;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = player.GetType();

            string[] candidateProperties =
            {
                "PersistentPlayerID",
                "PersistentPlayerId",
                "PlatformId",
                "CrossplatformId",
                "EntityName"
            };

            for (int i = 0; i < candidateProperties.Length; i++)
            {
                PropertyInfo property = type.GetProperty(candidateProperties[i], flags);
                if (property == null || !property.CanRead)
                {
                    continue;
                }

                object value = property.GetValue(player, null);
                if (value != null)
                {
                    string id = value.ToString();
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        return id.Trim();
                    }
                }
            }

            string[] candidateMethods =
            {
                "GetPersistentPlayerID",
                "getPersistentPlayerID"
            };

            for (int i = 0; i < candidateMethods.Length; i++)
            {
                MethodInfo method = type.GetMethod(candidateMethods[i], flags, null, Type.EmptyTypes, null);
                if (method == null)
                {
                    continue;
                }

                object value = method.Invoke(player, null);
                if (value != null)
                {
                    string id = value.ToString();
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        return id.Trim();
                    }
                }
            }

            return null;
        }
    }
}
