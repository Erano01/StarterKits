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
        private sealed class StoreLocation
        {
            public string StoreFilePath;
            public string WorldName;
            public string SaveName;
            public bool IsSaveScoped;
        }

        private static readonly object Sync = new object();
        private static readonly HashSet<string> SelectedPlayers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> SessionSelectedPlayers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> SessionSelectedKitByPlayer = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<string> PendingSelectionLines = new List<string>();
        private static readonly string ModDataRootPath = Path.Combine(Environment.CurrentDirectory, "Mods", "StarterKits", "Data", "saves");
        private static string loadedFromPath;
        private static bool didLogKnownSaves;
        private static string cachedWorldName;
        private static string cachedSaveName;

        public static void OnNewGameDetected()
        {
            lock (Sync)
            {
                StoreLocation location = ResolveStoreLocation();
                if (!location.IsSaveScoped || string.IsNullOrWhiteSpace(location.StoreFilePath))
                {
                    Log.Warning("[StarterKits] OnNewGameDetected: save path not resolved yet, clearing in-memory session state only.");
                    SelectedPlayers.Clear();
                    SessionSelectedPlayers.Clear();
                    SessionSelectedKitByPlayer.Clear();
                    PendingSelectionLines.Clear();
                    loadedFromPath = null;
                    return;
                }

                // Delete the db file for this save so kit selection opens fresh.
                try
                {
                    if (File.Exists(location.StoreFilePath))
                    {
                        File.Delete(location.StoreFilePath);
                        Log.Out($"[StarterKits] New game detected. Cleared DB for world='{location.WorldName}', save='{location.SaveName}'.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"[StarterKits] Could not delete old DB on new game: {ex.Message}");
                }

                SelectedPlayers.Clear();
                SessionSelectedPlayers.Clear();
                SessionSelectedKitByPlayer.Clear();
                PendingSelectionLines.Clear();
                loadedFromPath = null;
            }
        }

        public static void OnGameStartDone()
        {
            string worldName;
            string saveName;

            // At GameStartDone the game is fully loaded; GamePrefs should be reliable.
            if (TryReadWorldAndSaveFromPrefs(out worldName, out saveName))
            {
                lock (Sync)
                {
                    if (!string.Equals(cachedWorldName, worldName, StringComparison.Ordinal) ||
                        !string.Equals(cachedSaveName, saveName, StringComparison.Ordinal))
                    {
                        cachedWorldName = worldName;
                        cachedSaveName = saveName;
                        // Reset loadedFromPath so EnsureLoaded re-binds to the correct save DB.
                        loadedFromPath = null;
                        Log.Out($"[StarterKits] World/save context updated: world='{cachedWorldName}', save='{cachedSaveName}'");
                    }
                }
            }

            InitializeOnModLoad();
        }

        public static void InitializeOnModLoad()
        {
            lock (Sync)
            {
                if (!didLogKnownSaves)
                {
                    StoreLocation location = ResolveStoreLocation();
                    LogKnownSaves(location);
                    didLogKnownSaves = true;
                }
            }

            EnsureLoaded();
        }

        private static string FilePath
        {
            get
            {
                StoreLocation location = ResolveStoreLocation();
                if (!location.IsSaveScoped || string.IsNullOrWhiteSpace(location.StoreFilePath))
                {
                    // Keep a deterministic fallback path, but do not treat it as persisted game data.
                    return Path.Combine(Environment.CurrentDirectory, "Mods", "StarterKits", "Data", "pending-session-db.txt");
                }

                return location.StoreFilePath;
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
                return SessionSelectedPlayers.Contains(stableId) || SelectedPlayers.Contains(stableId);
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
                if (SessionSelectedKitByPlayer.TryGetValue(stableId, out string sessionKit))
                {
                    return sessionKit;
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
                Log.Warning("[StarterKits] Could not resolve stable player id; one-time lock could not be persisted.");
                return;
            }

            EnsureLoaded();

            lock (Sync)
            {
                if (!SessionSelectedPlayers.Add(stableId))
                {
                    return;
                }

                SessionSelectedKitByPlayer[stableId] = kitName ?? string.Empty;
                SelectedPlayers.Add(stableId);

                string line = BuildStoreLine(stableId, kitName);
                if (!TryAppendLineToSaveStore(line))
                {
                    PendingSelectionLines.Add(line);
                    Log.Warning("[StarterKits] Active save path not resolved yet. Selection queued in memory and will be flushed when save is known.");
                }
            }
        }

        private static string BuildStoreLine(string stableId, string kitName)
        {
            return stableId + "|" + (kitName ?? string.Empty) + "|" + DateTime.UtcNow.ToString("o");
        }

        private static bool TryAppendLineToSaveStore(string line)
        {
            StoreLocation location = ResolveStoreLocation();
            if (!location.IsSaveScoped || string.IsNullOrWhiteSpace(location.StoreFilePath))
            {
                return false;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(location.StoreFilePath));
                using (var writer = File.AppendText(location.StoreFilePath))
                {
                    writer.WriteLine(line);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] Could not append selection store entry: {ex.Message}");
                return false;
            }
        }

        private static void FlushPendingSelections()
        {
            if (PendingSelectionLines.Count == 0)
            {
                return;
            }

            StoreLocation location = ResolveStoreLocation();
            if (!location.IsSaveScoped || string.IsNullOrWhiteSpace(location.StoreFilePath))
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(location.StoreFilePath));
                using (var writer = File.AppendText(location.StoreFilePath))
                {
                    for (int i = 0; i < PendingSelectionLines.Count; i++)
                    {
                        writer.WriteLine(PendingSelectionLines[i]);
                    }
                }

                Log.Out($"[StarterKits] Flushed {PendingSelectionLines.Count} pending selection record(s) into save DB: {location.StoreFilePath}");
                PendingSelectionLines.Clear();
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] Could not flush pending selections: {ex.Message}");
            }
        }

        private static void EnsureLoaded()
        {
            StoreLocation location = ResolveStoreLocation();
            if (!location.IsSaveScoped || string.IsNullOrWhiteSpace(location.StoreFilePath))
            {
                return;
            }

            if (string.Equals(loadedFromPath, location.StoreFilePath, StringComparison.Ordinal))
            {
                lock (Sync)
                {
                    FlushPendingSelections();
                }

                return;
            }

            lock (Sync)
            {
                if (string.Equals(loadedFromPath, location.StoreFilePath, StringComparison.Ordinal))
                {
                    FlushPendingSelections();
                    return;
                }

                SelectedPlayers.Clear();

                try
                {
                    LoadEntriesFromFile(location.StoreFilePath);
                }
                catch (Exception ex)
                {
                    Log.Warning($"[StarterKits] Could not load selection store: {ex.Message}");
                }

                loadedFromPath = location.StoreFilePath;
                Log.Out($"[StarterKits] Active save DB bound to world='{location.WorldName}', save='{location.SaveName}', path='{location.StoreFilePath}'");
                FlushPendingSelections();
            }
        }

        private static void LoadEntriesFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            foreach (var line in File.ReadAllLines(path))
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

        private static StoreLocation ResolveStoreLocation()
        {
            string worldName;
            string saveName;
            bool hasWorldAndSave = TryResolveWorldAndSaveNames(out worldName, out saveName);

            var location = new StoreLocation
            {
                WorldName = worldName,
                SaveName = saveName,
                IsSaveScoped = hasWorldAndSave
            };

            if (!location.IsSaveScoped)
            {
                return location;
            }

            location.StoreFilePath = Path.Combine(ModDataRootPath, SanitizePathSegment(worldName), SanitizePathSegment(saveName), "db.txt");
            return location;
        }

        private static bool TryResolveWorldAndSaveNames(out string worldName, out string saveName)
        {
            // Prefer cached values set by OnGameStartDone (most reliable).
            if (!string.IsNullOrWhiteSpace(cachedWorldName) && !string.IsNullOrWhiteSpace(cachedSaveName))
            {
                worldName = cachedWorldName;
                saveName = cachedSaveName;
                return true;
            }

            worldName = null;
            saveName = null;

            string cachedRoot = TryReadGameIOCachedSaveRootPath();
            if (!string.IsNullOrWhiteSpace(cachedRoot) && TryExtractWorldAndSaveFromPath(cachedRoot, out worldName, out saveName))
            {
                return true;
            }

            return TryReadWorldAndSaveFromPrefs(out worldName, out saveName);
        }

        private static string TryReadGameIOCachedSaveRootPath()
        {
            try
            {
                Type gameIOType = Type.GetType("GameIO") ?? typeof(GameManager).Assembly.GetType("GameIO");
                if (gameIOType == null)
                {
                    return null;
                }

                FieldInfo field = gameIOType.GetField("m_CachedSaveGameRootDir", BindingFlags.Public | BindingFlags.Static);
                if (field == null)
                {
                    return null;
                }

                string path = field.GetValue(null) as string;
                return NormalizePath(path);
            }
            catch
            {
                return null;
            }
        }

        private static bool TryReadWorldAndSaveFromPrefs(out string worldName, out string saveName)
        {
            worldName = null;
            saveName = null;

            try
            {
                Type prefsType = Type.GetType("GamePrefs") ?? typeof(GameManager).Assembly.GetType("GamePrefs");
                Type enumType = Type.GetType("EnumGamePrefs") ?? typeof(GameManager).Assembly.GetType("EnumGamePrefs");
                if (prefsType == null || enumType == null)
                {
                    return false;
                }

                MethodInfo getString = prefsType.GetMethod("GetString", BindingFlags.Public | BindingFlags.Static);
                if (getString == null)
                {
                    return false;
                }

                worldName = TryReadPrefString(getString, enumType, "GameWorld");
                saveName = TryReadPrefString(getString, enumType, "GameName");
                if (string.IsNullOrWhiteSpace(worldName) || string.IsNullOrWhiteSpace(saveName))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                worldName = null;
                saveName = null;
                return false;
            }
        }

        private static string TryReadPrefString(MethodInfo getString, Type enumType, string enumName)
        {
            try
            {
                object enumValue = Enum.Parse(enumType, enumName, true);
                object value = getString.Invoke(null, new[] { enumValue });
                return value as string;
            }
            catch
            {
                return null;
            }
        }

        private static bool TryExtractWorldAndSaveFromPath(string path, out string worldName, out string saveName)
        {
            worldName = null;
            saveName = null;

            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            string normalized = NormalizePath(path);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            string[] segments = normalized.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 3)
            {
                return false;
            }

            int savesIndex = -1;
            for (int i = 0; i < segments.Length; i++)
            {
                if (string.Equals(segments[i], "Saves", StringComparison.OrdinalIgnoreCase))
                {
                    savesIndex = i;
                }
            }

            if (savesIndex < 0 || savesIndex + 2 >= segments.Length)
            {
                return false;
            }

            worldName = segments[savesIndex + 1];
            saveName = segments[savesIndex + 2];
            return !string.IsNullOrWhiteSpace(worldName) && !string.IsNullOrWhiteSpace(saveName);
        }

        private static void LogKnownSaves(StoreLocation location)
        {
            try
            {
                if (location == null || !location.IsSaveScoped)
                {
                    Log.Warning("[StarterKits] Save scan skipped: active world/save could not be resolved yet.");
                    return;
                }

                Log.Out($"[StarterKits] Active slot: world='{location.WorldName}', save='{location.SaveName}', store='{location.StoreFilePath}'");

                if (!Directory.Exists(ModDataRootPath))
                {
                    Log.Out($"[StarterKits] Save scan complete. worlds=0, saves=0, root='{ModDataRootPath}'");
                    return;
                }

                string[] worldDirs = Directory.GetDirectories(ModDataRootPath);
                int worldCount = worldDirs.Length;
                int saveCount = 0;
                var entries = new List<string>();

                for (int i = 0; i < worldDirs.Length; i++)
                {
                    string worldDir = worldDirs[i];
                    string worldName = Path.GetFileName(worldDir);
                    string[] saveDirs;
                    try
                    {
                        saveDirs = Directory.GetDirectories(worldDir);
                    }
                    catch
                    {
                        continue;
                    }

                    for (int s = 0; s < saveDirs.Length; s++)
                    {
                        string saveName = Path.GetFileName(saveDirs[s]);
                        saveCount++;
                        entries.Add(worldName + "/" + saveName);
                    }
                }

                Log.Out($"[StarterKits] Save scan complete. worlds={worldCount}, saves={saveCount}, root='{ModDataRootPath}'");
                if (entries.Count == 0)
                {
                    return;
                }

                int max = entries.Count > 30 ? 30 : entries.Count;
                for (int i = 0; i < max; i++)
                {
                    Log.Out($"[StarterKits] Save slot {i + 1}: {entries[i]}");
                }

                if (entries.Count > max)
                {
                    Log.Out($"[StarterKits] Save scan list truncated: {entries.Count - max} more slot(s).");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] Save scan failed: {ex.Message}");
            }
        }

        private static string SanitizePathSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "unknown";
            }

            char[] invalid = Path.GetInvalidFileNameChars();
            char[] chars = value.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                for (int j = 0; j < invalid.Length; j++)
                {
                    if (chars[i] == invalid[j])
                    {
                        chars[i] = '_';
                        break;
                    }
                }
            }

            string sanitized = new string(chars).Trim();
            return string.IsNullOrWhiteSpace(sanitized) ? "unknown" : sanitized;
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                return Path.GetFullPath(path.Trim());
            }
            catch
            {
                return null;
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
