using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace StarterKits.Kits
{
    /// <summary>
    /// Reads and writes kit definitions.
    /// - Kits/defaults.xml : shipped with the mod, never written.
    /// - Kits/kits.xml     : the user's kits. Created from defaults on first load, written by the editor.
    /// </summary>
    public static class KitRepository
    {
        public const string DefaultsFileName = "defaults.xml";
        public const string UserFileName = "kits.xml";
        private const int SchemaVersion = 1;

        private static string modPath;

        /// <summary>Set from IModApi.InitMod so we never guess the folder name.</summary>
        public static string ModPath
        {
            get
            {
                if (string.IsNullOrEmpty(modPath))
                {
                    modPath = Path.Combine(Environment.CurrentDirectory, "Mods", "StarterKits");
                }

                return modPath;
            }
            set { modPath = value; }
        }

        public static string KitsDirectory => Path.Combine(ModPath, "Kits");
        public static string DefaultsFilePath => Path.Combine(KitsDirectory, DefaultsFileName);
        public static string UserFilePath => Path.Combine(KitsDirectory, UserFileName);

        /// <summary>
        /// Loads the user's kits. Falls back to defaults if kits.xml is missing or unreadable.
        /// </summary>
        public static List<KitDefinition> LoadKits(out string message)
        {
            // Connected to a multiplayer server: the server's kits win over anything on this PC.
            if (Network.KitSync.IsRemoteClient)
            {
                return Network.KitSync.GetServerKits(out message);
            }

            if (File.Exists(UserFilePath))
            {
                try
                {
                    List<KitDefinition> kits = ReadFile(UserFilePath);
                    message = $"Loaded {kits.Count} kit(s) from {UserFileName}.";
                    return kits;
                }
                catch (Exception ex)
                {
                    Log.Warning($"[StarterKits] Could not read '{UserFilePath}': {ex.Message}. Falling back to defaults.");
                    List<KitDefinition> fallback = LoadDefaults(out string _);
                    message = $"{UserFileName} is invalid ({ex.Message}). Showing defaults; saving will overwrite it.";
                    return fallback;
                }
            }

            List<KitDefinition> defaults = LoadDefaults(out string defaultsMessage);
            message = defaultsMessage;
            return defaults;
        }

        public static List<KitDefinition> LoadDefaults(out string message)
        {
            try
            {
                List<KitDefinition> kits = ReadFile(DefaultsFilePath);
                message = $"Loaded {kits.Count} default kit(s).";
                return kits;
            }
            catch (Exception ex)
            {
                Log.Error($"[StarterKits] Could not read default kits '{DefaultsFilePath}': {ex.Message}");
                message = $"Default kits could not be read: {ex.Message}";
                return new List<KitDefinition>();
            }
        }

        public static void SaveKits(IList<KitDefinition> kits)
        {
            Directory.CreateDirectory(KitsDirectory);
            XDocument doc = BuildDocument(kits);

            // Write to a temp file first so a crash mid-write cannot destroy the user's kits.
            string tempPath = UserFilePath + ".tmp";
            using (var writer = new StreamWriter(tempPath, false, new UTF8Encoding(false)))
            {
                doc.Save(writer);
            }

            if (File.Exists(UserFilePath))
            {
                File.Copy(UserFilePath, UserFilePath + ".bak", true);
                File.Delete(UserFilePath);
            }

            File.Move(tempPath, UserFilePath);
            Log.Out($"[StarterKits] Saved {kits.Count} kit(s) to '{UserFilePath}'.");
        }

        /// <summary>The kit list as XML text (what the server sends to clients).</summary>
        public static string ToXmlString(IList<KitDefinition> kits)
        {
            return BuildDocument(kits).ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>Parses kit XML received from the server.</summary>
        public static List<KitDefinition> ParseXml(string xml, string sourceName)
        {
            return ParseDocument(XDocument.Parse(xml), sourceName);
        }

        private static XDocument BuildDocument(IList<KitDefinition> kits)
        {
            var root = new XElement("starterkits", new XAttribute("version", SchemaVersion));
            foreach (KitDefinition kit in kits)
            {
                root.Add(WriteKit(kit));
            }

            return new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XComment(" StarterKits user kits. Edited by the in-game Kit Editor (main menu) - hand edits are fine too. "),
                root);
        }

        private static List<KitDefinition> ReadFile(string path)
        {
            return ParseDocument(XDocument.Load(path), path);
        }

        private static List<KitDefinition> ParseDocument(XDocument doc, string path)
        {
            XElement root = doc.Root;
            if (root == null || root.Name.LocalName != "starterkits")
            {
                throw new InvalidDataException("root element must be <starterkits>");
            }

            var kits = new List<KitDefinition>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (XElement kitElement in root.Elements("kit"))
            {
                KitDefinition kit = ReadKit(kitElement);
                if (string.IsNullOrWhiteSpace(kit.Id))
                {
                    Log.Warning($"[StarterKits] Skipping a <kit> without id in '{path}'.");
                    continue;
                }

                if (!seen.Add(kit.Id))
                {
                    Log.Warning($"[StarterKits] Duplicate kit id '{kit.Id}' in '{path}', keeping the first one.");
                    continue;
                }

                kits.Add(kit);
            }

            return kits;
        }

        private static KitDefinition ReadKit(XElement e)
        {
            var kit = new KitDefinition
            {
                Id = Attr(e, "id").Trim(),
                Name = Attr(e, "name"),
                Title = Attr(e, "title"),
                Image = Attr(e, "image"),
                ImageAtlas = Attr(e, "image_atlas", KitAtlases.ItemIcons),
                AutoStats = string.Equals(Attr(e, "auto_stats"), "true", StringComparison.OrdinalIgnoreCase),
                Description = (e.Element("description")?.Value ?? string.Empty).Trim()
            };

            if (string.IsNullOrEmpty(kit.Name))
            {
                kit.Name = kit.Id;
            }

            foreach (XElement p in e.Elements("progression"))
            {
                string name = Attr(p, "name").Trim();
                if (name.Length == 0)
                {
                    continue;
                }

                kit.Progressions.Add(new KitProgression { Name = name, Level = ParseInt(Attr(p, "level"), 1) });
            }

            foreach (XElement i in e.Elements("item"))
            {
                string name = Attr(i, "name").Trim();
                if (name.Length == 0)
                {
                    continue;
                }

                kit.Items.Add(new KitItem
                {
                    Name = name,
                    Count = Math.Max(1, ParseInt(Attr(i, "count"), 1)),
                    Quality = Clamp(ParseInt(Attr(i, "quality"), 0), 0, 6)
                });
            }

            foreach (XElement c in e.Elements("cvar"))
            {
                string name = Attr(c, "name").Trim();
                if (name.Length == 0)
                {
                    continue;
                }

                float value;
                float.TryParse(Attr(c, "value", "0"), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
                kit.Cvars.Add(new KitCvar { Name = name, Value = value });
            }

            foreach (XElement s in e.Elements("stat"))
            {
                kit.Stats.Add(new KitStat
                {
                    Text = Attr(s, "text"),
                    Icon = Attr(s, "icon"),
                    Atlas = Attr(s, "atlas", KitAtlases.Ui),
                    Color = Attr(s, "color")
                });
            }

            return kit;
        }

        private static XElement WriteKit(KitDefinition kit)
        {
            var e = new XElement("kit",
                new XAttribute("id", kit.Id ?? string.Empty),
                new XAttribute("name", kit.Name ?? string.Empty),
                new XAttribute("title", kit.Title ?? string.Empty),
                new XAttribute("image", kit.Image ?? string.Empty),
                new XAttribute("image_atlas", string.IsNullOrEmpty(kit.ImageAtlas) ? KitAtlases.ItemIcons : kit.ImageAtlas));

            if (kit.AutoStats)
            {
                e.Add(new XAttribute("auto_stats", "true"));
            }

            e.Add(new XElement("description", kit.Description ?? string.Empty));

            foreach (KitProgression p in kit.Progressions)
            {
                e.Add(new XElement("progression",
                    new XAttribute("name", p.Name),
                    new XAttribute("level", p.Level.ToString(CultureInfo.InvariantCulture))));
            }

            foreach (KitItem item in kit.Items)
            {
                var itemElement = new XElement("item",
                    new XAttribute("name", item.Name),
                    new XAttribute("count", item.Count.ToString(CultureInfo.InvariantCulture)));
                if (item.Quality > 0)
                {
                    itemElement.Add(new XAttribute("quality", item.Quality.ToString(CultureInfo.InvariantCulture)));
                }

                e.Add(itemElement);
            }

            foreach (KitCvar cvar in kit.Cvars)
            {
                e.Add(new XElement("cvar",
                    new XAttribute("name", cvar.Name),
                    new XAttribute("value", cvar.Value.ToString(CultureInfo.InvariantCulture))));
            }

            foreach (KitStat stat in kit.Stats)
            {
                var statElement = new XElement("stat", new XAttribute("text", stat.Text ?? string.Empty));
                if (!string.IsNullOrEmpty(stat.Icon))
                {
                    statElement.Add(new XAttribute("icon", stat.Icon));
                }

                if (!string.IsNullOrEmpty(stat.Atlas) && stat.Atlas != KitAtlases.Ui)
                {
                    statElement.Add(new XAttribute("atlas", stat.Atlas));
                }

                if (!string.IsNullOrEmpty(stat.Color))
                {
                    statElement.Add(new XAttribute("color", stat.Color));
                }

                e.Add(statElement);
            }

            return e;
        }

        private static string Attr(XElement e, string name, string fallback = "")
        {
            XAttribute a = e.Attribute(name);
            return a != null ? a.Value : fallback;
        }

        private static int ParseInt(string s, int fallback)
        {
            int value;
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : fallback;
        }

        public static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }
    }
}
