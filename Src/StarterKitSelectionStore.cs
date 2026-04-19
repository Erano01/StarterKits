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
        private static bool loaded;

        private static string FilePath => Path.Combine(Environment.CurrentDirectory, "Mods", "StarterKits", "Data", "selected-players.txt");

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
            if (loaded)
            {
                return;
            }

            lock (Sync)
            {
                if (loaded)
                {
                    return;
                }

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

                loaded = true;
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
