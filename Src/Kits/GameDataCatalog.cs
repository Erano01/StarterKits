using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace StarterKits.Kits
{
    public enum ProgressionCategory
    {
        Attribute,
        Skill,
        Perk,
        BookSeries,
        Book,
        Crafting
    }

    public sealed class ProgressionInfo
    {
        public string Name;
        public string Parent;
        public ProgressionCategory Category;
        public int MaxLevel;
        public string Icon;
        public string DisplayName;
        public float SortOrder;

        /// <summary>Attribute this entry lives under (perks: via their skill category). Null for books/magazines.</summary>
        public ProgressionInfo Attribute;

        /// <summary>Perks: their skill category, e.g. "Combat Perks" under Agility. Books: their book set.</summary>
        public ProgressionInfo Group;

        /// <summary>Human readable location, e.g. "Agility > Combat" or "Book: Sniper".</summary>
        public string Path = string.Empty;

        /// <summary>For a BookSeries entry: its books, in file order (the "...Complete" book last).</summary>
        public readonly List<ProgressionInfo> Books = new List<ProgressionInfo>();
    }

    public sealed class ItemInfo
    {
        public string Name;
        public string Icon;
        public string DisplayName;

        /// <summary>"Item", "Mod" (item_modifiers.xml) or "Block" (player-placeable block).</summary>
        public string Kind = "Item";

        public bool IsBlock => Kind == "Block";
    }

    /// <summary>
    /// Every progression (attribute / skill / perk / book / crafting-magazine) and item in the game,
    /// with display names and icons.
    ///
    /// The main menu runs before the game loads progression.xml / items.xml (they are loaded at
    /// StartGame), so Progression.ProgressionClasses and ItemClass are empty there. We read the
    /// vanilla files from Data/Config ourselves. Localization and the UI atlases ARE loaded at boot.
    /// Limitation: progressions/items added by other mods' XML patches are not listed.
    /// </summary>
    public static class GameDataCatalog
    {
        private static readonly Dictionary<string, ProgressionInfo> ProgressionsByName =
            new Dictionary<string, ProgressionInfo>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<ProgressionInfo> ProgressionList = new List<ProgressionInfo>();

        private static readonly Dictionary<string, ItemInfo> ItemsByName =
            new Dictionary<string, ItemInfo>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<ItemInfo> ItemList = new List<ItemInfo>();

        private const string NoInheritKey = "__noinherit";
        private static bool progressionsLoaded;
        private static bool itemsLoaded;

        public static IReadOnlyList<ProgressionInfo> Progressions
        {
            get
            {
                EnsureProgressions();
                return ProgressionList;
            }
        }

        public static ProgressionInfo FindProgression(string name)
        {
            EnsureProgressions();
            ProgressionInfo info;
            return name != null && ProgressionsByName.TryGetValue(name, out info) ? info : null;
        }

        public static ItemInfo FindItem(string name)
        {
            EnsureItems();
            ItemInfo info;
            return name != null && ItemsByName.TryGetValue(name.Trim(), out info) ? info : null;
        }

        /// <summary>All items and player-placeable blocks, sorted by display name.</summary>
        public static IReadOnlyList<ItemInfo> Items
        {
            get
            {
                EnsureItems();
                return ItemList;
            }
        }

        private static string ConfigPath(string fileName)
        {
            return Path.Combine(GameIO.GetGameDir("Data/Config"), fileName);
        }

        private static void EnsureProgressions()
        {
            if (progressionsLoaded)
            {
                return;
            }

            progressionsLoaded = true;
            try
            {
                LoadProgressions(XDocument.Load(ConfigPath("progression.xml")));
                Log.Out($"[StarterKits] Kit editor catalog: {ProgressionList.Count} progressions loaded.");
            }
            catch (Exception ex)
            {
                Log.Error($"[StarterKits] Could not read progression.xml for the kit editor: {ex.Message}");
            }
        }

        private static void LoadProgressions(XDocument doc)
        {
            XElement root = doc.Root;
            if (root == null)
            {
                return;
            }

            // Container defaults, mirroring ProgressionFromXml.parseProgressionItems.
            var containerMax = new Dictionary<string, int>
            {
                ["attributes"] = 10,
                ["skills"] = 100,
                ["perks"] = 5,
                ["crafting_skills"] = 100
            };

            float order = 0f;
            foreach (XElement container in root.Elements())
            {
                string containerName = container.Name.LocalName;
                if (!containerMax.ContainsKey(containerName))
                {
                    continue;
                }

                int defaultMax = ParseInt(Attr(container, "max_level"), containerMax[containerName]);
                foreach (XElement e in container.Elements())
                {
                    ProgressionCategory category;
                    if (!TryGetCategory(e.Name.LocalName, out category))
                    {
                        continue;
                    }

                    string name = Attr(e, "name");
                    if (string.IsNullOrEmpty(name) || ProgressionsByName.ContainsKey(name))
                    {
                        continue;
                    }

                    var info = new ProgressionInfo
                    {
                        Name = name,
                        Parent = Attr(e, "parent"),
                        Category = category,
                        MaxLevel = ParseInt(Attr(e, "max_level"), defaultMax),
                        Icon = Attr(e, "icon"),
                        SortOrder = order++
                    };
                    info.DisplayName = ResolveDisplayName(info, Attr(e, "name_key"));

                    ProgressionsByName[name] = info;
                    ProgressionList.Add(info);
                }
            }

            // Books: inherit the series icon and attach to their series.
            foreach (ProgressionInfo info in ProgressionList)
            {
                if (info.Category != ProgressionCategory.Book)
                {
                    continue;
                }

                ProgressionInfo series;
                if (!string.IsNullOrEmpty(info.Parent) && ProgressionsByName.TryGetValue(info.Parent, out series))
                {
                    series.Books.Add(info);
                    if (string.IsNullOrEmpty(info.Icon))
                    {
                        info.Icon = series.Icon;
                    }
                }
            }

            foreach (ProgressionInfo info in ProgressionList)
            {
                ResolveHierarchy(info);

                if (info.Category == ProgressionCategory.BookSeries)
                {
                    info.MaxLevel = CountCollectibleBooks(info);
                }

                if (string.IsNullOrEmpty(info.Icon))
                {
                    info.Icon = "ui_game_symbol_book";
                }
            }
        }

        private static void ResolveHierarchy(ProgressionInfo info)
        {
            ProgressionInfo parent = FindLoaded(info.Parent);
            switch (info.Category)
            {
                case ProgressionCategory.Perk:
                    info.Group = parent;
                    info.Attribute = parent != null ? FindLoaded(parent.Parent) : null;
                    if (info.Attribute != null && string.Equals(info.Attribute.Name, "attGeneralPerks", StringComparison.OrdinalIgnoreCase))
                    {
                        info.Path = "General";
                    }
                    else
                    {
                        info.Path = info.Attribute != null
                            ? info.Attribute.DisplayName + " > " + ShortGroupName(parent)
                            : ShortGroupName(parent);
                    }

                    break;
                case ProgressionCategory.Skill:
                    info.Attribute = parent;
                    info.Path = parent != null ? parent.DisplayName : string.Empty;
                    break;
                case ProgressionCategory.Book:
                    info.Group = parent;
                    info.Path = parent != null ? "Book: " + parent.DisplayName : "Book";
                    break;
                case ProgressionCategory.BookSeries:
                    info.Path = "Book set";
                    break;
                case ProgressionCategory.Crafting:
                    info.Path = "Magazine";
                    break;
                case ProgressionCategory.Attribute:
                    info.Path = "Attribute";
                    break;
            }
        }

        /// <summary>"Combat Perks" -> "Combat", "General Perception Perks" -> "General".</summary>
        public static string ShortGroupName(ProgressionInfo group)
        {
            if (group == null)
            {
                return string.Empty;
            }

            string name = group.DisplayName ?? string.Empty;
            if (name.EndsWith(" Perks", StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - " Perks".Length);
            }

            if (group.Name.EndsWith("General", StringComparison.Ordinal) || group.Name.EndsWith("Craftsmanship", StringComparison.Ordinal))
            {
                return "General";
            }

            return name;
        }

        private static ProgressionInfo FindLoaded(string name)
        {
            ProgressionInfo info;
            return !string.IsNullOrEmpty(name) && ProgressionsByName.TryGetValue(name, out info) ? info : null;
        }

        /// <summary>Books in a series excluding the "...Complete" bonus (what the game shows as "7/7").</summary>
        public static int CountCollectibleBooks(ProgressionInfo series)
        {
            int count = 0;
            foreach (ProgressionInfo book in series.Books)
            {
                if (!IsCompletionBook(book))
                {
                    count++;
                }
            }

            return count;
        }

        public static bool IsCompletionBook(ProgressionInfo book)
        {
            return book.Name.EndsWith("Complete", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryGetCategory(string elementName, out ProgressionCategory category)
        {
            switch (elementName)
            {
                case "attribute": category = ProgressionCategory.Attribute; return true;
                case "skill": category = ProgressionCategory.Skill; return true;
                case "perk": category = ProgressionCategory.Perk; return true;
                case "book_group": category = ProgressionCategory.BookSeries; return true;
                case "book": category = ProgressionCategory.Book; return true;
                case "crafting_skill": category = ProgressionCategory.Crafting; return true;
                default: category = ProgressionCategory.Perk; return false;
            }
        }

        private static string ResolveDisplayName(ProgressionInfo info, string nameKey)
        {
            string text;
            if (TryLocalize(nameKey, out text) || TryLocalize(info.Name + "Name", out text) || TryLocalize(info.Name, out text))
            {
                return text;
            }

            // Books without name_key use the book item's key: perkSniperDamage -> bookSniperDamage.
            if (info.Category == ProgressionCategory.Book && info.Name.StartsWith("perk", StringComparison.Ordinal) &&
                TryLocalize("book" + info.Name.Substring(4), out text))
            {
                return text;
            }

            return Prettify(info.Name);
        }

        private static bool TryLocalize(string key, out string text)
        {
            text = null;
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            try
            {
                if (!Localization.Exists(key))
                {
                    return false;
                }

                text = Localization.Get(key);
                return !string.IsNullOrEmpty(text) && text != key;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>"perkDeadEye" -> "Dead Eye".</summary>
        public static string Prettify(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            string[] prefixes = { "perk", "skill", "att", "crafting", "book" };
            foreach (string prefix in prefixes)
            {
                if (name.Length > prefix.Length && name.StartsWith(prefix, StringComparison.Ordinal) && char.IsUpper(name[prefix.Length]))
                {
                    name = name.Substring(prefix.Length);
                    break;
                }
            }

            var sb = new StringBuilder(name.Length + 8);
            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
                {
                    sb.Append(' ');
                }

                sb.Append(name[i]);
            }

            return sb.ToString();
        }

        private static void EnsureItems()
        {
            if (itemsLoaded)
            {
                return;
            }

            itemsLoaded = true;
            try
            {
                LoadItemFile("items.xml", "item", "Item");
                LoadItemFile("item_modifiers.xml", "item_modifier", "Mod");
                LoadItemFile("blocks.xml", "block", "Block");
                ItemList.AddRange(ItemsByName.Values);
                ItemList.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.OrdinalIgnoreCase));
                Log.Out($"[StarterKits] Kit editor catalog: {ItemList.Count} items/blocks loaded.");
            }
            catch (Exception ex)
            {
                Log.Error($"[StarterKits] Could not read items/blocks for the kit editor: {ex.Message}");
            }
        }

        private static void LoadItemFile(string fileName, string elementName, string kind)
        {
            bool blocks = kind == "Block";
            XDocument doc = XDocument.Load(ConfigPath(fileName));
            var props = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            var order = new List<string>();

            foreach (XElement e in doc.Root.Elements(elementName))
            {
                string name = Attr(e, "name");
                if (string.IsNullOrEmpty(name) || props.ContainsKey(name))
                {
                    continue;
                }

                var p = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (XElement prop in e.Elements("property"))
                {
                    string propName = Attr(prop, "name");
                    if (propName.Length > 0 && !p.ContainsKey(propName))
                    {
                        p[propName] = Attr(prop, "value");
                    }

                    // <property name="Extends" value="x" param1="CustomIcon,..."/> = inherit everything EXCEPT the listed properties.
                    if (propName == "Extends")
                    {
                        p[NoInheritKey] = "," + Attr(prop, "param1").Replace(" ", string.Empty) + ",";
                    }
                }

                props[name] = p;
                order.Add(name);
            }

            foreach (string name in order)
            {
                // Blocks: only what a player can place (workbenches, chests, ...); skip POI/dev/shape helper blocks.
                if (blocks && !string.Equals(Inherited(props, name, "CreativeMode"), "Player", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (ItemsByName.ContainsKey(name))
                {
                    continue;
                }

                string icon = Inherited(props, name, "CustomIcon");
                string text;
                ItemsByName[name] = new ItemInfo
                {
                    Name = name,
                    Kind = kind,
                    Icon = string.IsNullOrEmpty(icon) ? name : icon,
                    DisplayName = TryLocalize(name, out text) ? text : name
                };
            }
        }

        private static string Inherited(Dictionary<string, Dictionary<string, string>> props, string name, string key)
        {
            string current = name;
            for (int depth = 0; depth < 15 && !string.IsNullOrEmpty(current); depth++)
            {
                Dictionary<string, string> p;
                if (!props.TryGetValue(current, out p))
                {
                    return null;
                }

                string value;
                if (p.TryGetValue(key, out value))
                {
                    return value;
                }

                string excluded;
                if (p.TryGetValue(NoInheritKey, out excluded) && excluded.IndexOf("," + key + ",", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return null;
                }

                p.TryGetValue("Extends", out current);
            }

            return null;
        }

        private static string Attr(XElement e, string name)
        {
            XAttribute a = e.Attribute(name);
            return a != null ? a.Value : string.Empty;
        }

        private static int ParseInt(string s, int fallback)
        {
            int value;
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : fallback;
        }
    }
}
