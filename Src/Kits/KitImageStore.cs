using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace StarterKits.Kits
{
    /// <summary>
    /// Kit portraits imported by the player.
    ///
    /// UI atlases (UIAtlases/ItemIconAtlas) are packed once at game boot, so a PNG added there
    /// only shows up after a restart. Imported portraits are therefore stored as plain files in
    /// Kits/Images/ and shown through an XUi &lt;texture&gt; instead of an atlas sprite
    /// (kit attribute image_atlas="File").
    /// </summary>
    public static class KitImageStore
    {
        /// <summary>Same size as the built-in kit portraits.</summary>
        public const int PortraitSize = 200;

        private static readonly string[] ImportExtensions = { ".png", ".jpg", ".jpeg" };
        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        public static string ImagesDirectory => Path.Combine(KitRepository.KitsDirectory, "Images");

        /// <summary>Drop source pictures here; the import dialog lists them.</summary>
        public static string ImportDirectory => Path.Combine(KitRepository.KitsDirectory, "ImageImport");

        public static string PathFor(string imageName)
        {
            return Path.Combine(ImagesDirectory, imageName + ".png");
        }

        /// <summary>Imported portrait names (without extension), sorted.</summary>
        public static List<string> ListImages()
        {
            var names = new List<string>();
            try
            {
                if (Directory.Exists(ImagesDirectory))
                {
                    foreach (string file in Directory.GetFiles(ImagesDirectory, "*.png"))
                    {
                        names.Add(Path.GetFileNameWithoutExtension(file));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] Could not list kit images: {ex.Message}");
            }

            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        /// <summary>Pictures waiting in the import folder (png/jpg), sorted by name.</summary>
        public static List<string> ListImportCandidates()
        {
            var files = new List<string>();
            try
            {
                Directory.CreateDirectory(ImportDirectory);
                foreach (string file in Directory.GetFiles(ImportDirectory))
                {
                    if (IsSupported(file))
                    {
                        files.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] Could not list import folder: {ex.Message}");
            }

            files.Sort(StringComparer.OrdinalIgnoreCase);
            return files;
        }

        public static bool IsSupported(string path)
        {
            string ext = Path.GetExtension(path) ?? string.Empty;
            foreach (string allowed in ImportExtensions)
            {
                if (string.Equals(ext, allowed, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Loads any png/jpg from disk as a readable texture. Caller owns the result.</summary>
        public static Texture2D LoadSource(string path, out string error)
        {
            error = null;
            try
            {
                if (!File.Exists(path))
                {
                    error = "file not found";
                    return null;
                }

                if (!IsSupported(path))
                {
                    error = "only .png and .jpg files are supported";
                    return null;
                }

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(File.ReadAllBytes(path), false))
                {
                    UnityEngine.Object.Destroy(texture);
                    error = "not a valid image";
                    return null;
                }

                texture.wrapMode = TextureWrapMode.Clamp;
                return texture;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        /// <summary>Portrait texture for a kit with image_atlas="File" (cached; null if missing).</summary>
        public static Texture2D GetPortrait(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return null;
            }

            Texture2D texture;

            // On a multiplayer client, pictures come from the server (never from this PC's Kits/Images).
            if (Network.KitSync.IsRemoteClient)
            {
                return Network.KitSync.GetServerImage(imageName);
            }

            if (Cache.TryGetValue(imageName, out texture) && texture != null)
            {
                return texture;
            }

            string error;
            texture = LoadSource(PathFor(imageName), out error);
            if (texture == null)
            {
                Log.Warning($"[StarterKits] Kit image '{imageName}' could not be loaded: {error}");
                return null;
            }

            texture.name = "StarterKitImage_" + imageName;
            Cache[imageName] = texture;
            return texture;
        }

        /// <summary>
        /// Crops a square from the source (texture coordinates, origin bottom-left), scales it to
        /// PortraitSize with a box filter and writes Kits/Images/&lt;name&gt;.png.
        /// </summary>
        public static void SavePortrait(Texture2D source, float cropX, float cropY, float cropSize, string imageName)
        {
            int size = PortraitSize;
            var result = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            // Average several samples per output pixel when shrinking a big crop (avoids jagged edges).
            int samples = Mathf.Clamp(Mathf.CeilToInt(cropSize / size), 1, 6);
            float step = cropSize / size;
            float inv = 1f / (samples * samples);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color sum = Color.clear;
                    for (int sy = 0; sy < samples; sy++)
                    {
                        for (int sx = 0; sx < samples; sx++)
                        {
                            float px = cropX + (x + (sx + 0.5f) / samples) * step;
                            float py = cropY + (y + (sy + 0.5f) / samples) * step;
                            sum += source.GetPixelBilinear(px / source.width, py / source.height);
                        }
                    }

                    pixels[y * size + x] = sum * inv;
                }
            }

            result.SetPixels(pixels);
            result.Apply();

            Directory.CreateDirectory(ImagesDirectory);
            File.WriteAllBytes(PathFor(imageName), result.EncodeToPNG());
            UnityEngine.Object.Destroy(result);

            // Drop the cached copy so the new picture shows immediately.
            Texture2D old;
            if (Cache.TryGetValue(imageName, out old))
            {
                Cache.Remove(imageName);
                if (old != null)
                {
                    UnityEngine.Object.Destroy(old);
                }
            }

            Log.Out($"[StarterKits] Saved kit image '{PathFor(imageName)}'.");
        }

        /// <summary>"My Kit (1).jpg" -> "My_Kit_1". Letters, digits, '_' and '-' only.</summary>
        public static string SanitizeName(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(raw.Length);
            foreach (char c in raw.Trim())
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '-')
                {
                    sb.Append(c);
                }
                else if (c == ' ' || c == '.')
                {
                    sb.Append('_');
                }
            }

            return sb.ToString().Trim('_');
        }
    }
}
