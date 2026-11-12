using System;
using System.Collections.Generic;
using System.IO;
using StarterKits.Kits;
using UnityEngine;

namespace StarterKits.Network
{
    /// <summary>
    /// Server -> client delivery of kits.xml and imported kit pictures (Kits/Images).
    ///
    /// Server (dedicated or hosting player): when a remote player is about to spawn, it reads its
    /// own Kits/kits.xml fresh from disk and sends it plus every picture those kits use.
    /// Client: keeps what it received in memory for this session and uses it instead of its local
    /// files. Nothing is written to the player's disk and players never copy files by hand.
    /// Players still need the mod itself installed (it contains code), like every DLL mod.
    /// </summary>
    public static class KitSync
    {
        /// <summary>Bytes per image package. Pictures are ~200x200 PNGs, usually 1-3 chunks.</summary>
        public const int ChunkSize = 32 * 1024;

        /// <summary>Safety limits for data coming over the network.</summary>
        public const int MaxImageBytes = 4 * 1024 * 1024;
        public const int MaxImages = 256;

        // ---- client state (this session only) ----
        private static List<KitDefinition> serverKits;
        private static readonly Dictionary<string, Texture2D> ServerImages = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, byte[][]> PendingImages = new Dictionary<string, byte[][]>(StringComparer.OrdinalIgnoreCase);

        // ---- server state ----
        private static readonly HashSet<ClientInfo> SentThisSession = new HashSet<ClientInfo>();

        /// <summary>Raised on the client when kits or a picture arrive, so an open kit window can refresh.</summary>
        public static event Action Changed;

        public static void Init()
        {
            ModEvents.PlayerSpawning.RegisterHandler(OnPlayerSpawning);
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(OnPlayerSpawnedInWorld);
            ModEvents.WorldShuttingDown.RegisterHandler(OnWorldShuttingDown);
        }

        /// <summary>True when this game is connected to someone else's server (not host, not single player, not main menu).</summary>
        public static bool IsRemoteClient
        {
            get
            {
                try
                {
                    ConnectionManager cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
                    return cm != null && cm.IsClient && !cm.IsServer;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static bool IsServer
        {
            get
            {
                try
                {
                    ConnectionManager cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
                    return cm != null && cm.IsServer;
                }
                catch
                {
                    return false;
                }
            }
        }

        // =====================================================================
        // Server
        // =====================================================================

        private static void OnPlayerSpawning(ref ModEvents.SPlayerSpawningData _data)
        {
            TrySendTo(_data.ClientInfo);
        }

        /// <summary>Fallback in case PlayerSpawning did not reach us for this client.</summary>
        private static void OnPlayerSpawnedInWorld(ref ModEvents.SPlayerSpawnedInWorldData _data)
        {
            TrySendTo(_data.ClientInfo);
        }

        private static void TrySendTo(ClientInfo client)
        {
            // ClientInfo is null for the host's own (local) player - it reads the files directly.
            if (client == null || !IsServer)
            {
                return;
            }

            lock (SentThisSession)
            {
                if (!SentThisSession.Add(client))
                {
                    return;
                }
            }

            try
            {
                SendTo(client);
            }
            catch (Exception ex)
            {
                Log.Error($"[StarterKits] Could not send kits to {client.playerName}: {ex}");
            }
        }

        private static void SendTo(ClientInfo client)
        {
            // Read fresh every time: admins can edit kits.xml / add pictures without restarting the server.
            List<KitDefinition> kits = KitRepository.LoadKits(out string loadMessage);
            string xml = KitRepository.ToXmlString(kits);

            var images = new List<string>();
            foreach (KitDefinition kit in kits)
            {
                if (kit.ImageAtlas == KitAtlases.File && !string.IsNullOrEmpty(kit.Image) &&
                    !images.Exists(n => string.Equals(n, kit.Image, StringComparison.OrdinalIgnoreCase)))
                {
                    images.Add(kit.Image);
                }
            }

            client.SendPackage(NetPackageManager.GetPackage<NetPackageStarterKitsData>().Setup(xml));

            int sentImages = 0;
            long sentBytes = 0;
            foreach (string name in images)
            {
                string path = KitImageStore.PathFor(name);
                if (!File.Exists(path))
                {
                    Log.Warning($"[StarterKits] Kit picture '{path}' is missing on the server; players will see no image for it.");
                    continue;
                }

                byte[] bytes = File.ReadAllBytes(path);
                if (bytes.Length > MaxImageBytes)
                {
                    Log.Warning($"[StarterKits] Kit picture '{name}' is too large to send ({bytes.Length} bytes).");
                    continue;
                }

                int chunks = Math.Max(1, (bytes.Length + ChunkSize - 1) / ChunkSize);
                for (int i = 0; i < chunks; i++)
                {
                    int offset = i * ChunkSize;
                    int length = Math.Min(ChunkSize, bytes.Length - offset);
                    var chunk = new byte[length];
                    Buffer.BlockCopy(bytes, offset, chunk, 0, length);
                    client.SendPackage(NetPackageManager.GetPackage<NetPackageStarterKitsImage>().Setup(name, i, chunks, chunk));
                }

                sentImages++;
                sentBytes += bytes.Length;
            }

            Log.Out($"[StarterKits] Sent {kits.Count} kit(s) and {sentImages} picture(s) ({sentBytes / 1024} KB) to {client.playerName}. ({loadMessage})");
        }

        private static void OnWorldShuttingDown(ref ModEvents.SWorldShuttingDownData _data)
        {
            lock (SentThisSession)
            {
                SentThisSession.Clear();
            }

            ResetClientState();
        }

        // =====================================================================
        // Client
        // =====================================================================

        public static List<KitDefinition> GetServerKits(out string message)
        {
            if (serverKits == null)
            {
                message = "Waiting for the server to send its kits...";
                return new List<KitDefinition>();
            }

            message = $"Using {serverKits.Count} kit(s) from the server.";
            return serverKits;
        }

        public static Texture2D GetServerImage(string name)
        {
            Texture2D texture;
            return name != null && ServerImages.TryGetValue(name, out texture) ? texture : null;
        }

        internal static void OnKitsReceived(string xml)
        {
            try
            {
                serverKits = KitRepository.ParseXml(xml, "server");
                ClearImages();
                Log.Out($"[StarterKits] Received {serverKits.Count} kit(s) from the server.");
            }
            catch (Exception ex)
            {
                serverKits = new List<KitDefinition>();
                Log.Error($"[StarterKits] Kits sent by the server could not be read: {ex.Message}");
            }

            RaiseChanged();
        }

        internal static void OnImageChunk(string name, int index, int count, byte[] data)
        {
            if (string.IsNullOrEmpty(name) || count <= 0 || index < 0 || index >= count ||
                (long)count * ChunkSize > MaxImageBytes + ChunkSize || data == null)
            {
                Log.Warning($"[StarterKits] Ignoring invalid kit picture chunk '{name}' {index}/{count}.");
                return;
            }

            byte[][] parts;
            if (!PendingImages.TryGetValue(name, out parts) || parts.Length != count)
            {
                if (PendingImages.Count + ServerImages.Count >= MaxImages)
                {
                    return;
                }

                parts = new byte[count][];
                PendingImages[name] = parts;
            }

            parts[index] = data;
            foreach (byte[] part in parts)
            {
                if (part == null)
                {
                    return;
                }
            }

            PendingImages.Remove(name);

            var all = new List<byte>();
            foreach (byte[] part in parts)
            {
                all.AddRange(part);
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(all.ToArray(), true))
            {
                UnityEngine.Object.Destroy(texture);
                Log.Warning($"[StarterKits] Kit picture '{name}' from the server is not a valid image.");
                return;
            }

            texture.name = "StarterKitServerImage_" + name;
            Texture2D old;
            if (ServerImages.TryGetValue(name, out old) && old != null)
            {
                UnityEngine.Object.Destroy(old);
            }

            ServerImages[name] = texture;
            RaiseChanged();
        }

        private static void ResetClientState()
        {
            serverKits = null;
            ClearImages();
        }

        private static void ClearImages()
        {
            foreach (Texture2D texture in ServerImages.Values)
            {
                if (texture != null)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }

            ServerImages.Clear();
            PendingImages.Clear();
        }

        private static void RaiseChanged()
        {
            try
            {
                Changed?.Invoke();
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] Kit window refresh after server sync failed: {ex.Message}");
            }
        }
    }
}
