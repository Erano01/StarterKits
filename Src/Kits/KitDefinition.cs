using System;
using System.Collections.Generic;

namespace StarterKits.Kits
{
    /// <summary>
    /// One starter kit, as stored in Kits/defaults.xml and Kits/kits.xml.
    /// </summary>
    public sealed class KitDefinition
    {
        public string Id = string.Empty;
        public string Name = string.Empty;
        public string Title = string.Empty;
        public string Description = string.Empty;
        public string Image = string.Empty;
        public string ImageAtlas = KitAtlases.ItemIcons;

        /// <summary>
        /// When true, the stat lines shown in the kit overview are generated from
        /// the progressions and items every time the kit is saved.
        /// </summary>
        public bool AutoStats;

        public readonly List<KitProgression> Progressions = new List<KitProgression>();
        public readonly List<KitItem> Items = new List<KitItem>();
        public readonly List<KitCvar> Cvars = new List<KitCvar>();
        public readonly List<KitStat> Stats = new List<KitStat>();

        public KitProgression FindProgression(string name)
        {
            for (int i = 0; i < Progressions.Count; i++)
            {
                if (string.Equals(Progressions[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return Progressions[i];
                }
            }

            return null;
        }

        public KitItem FindItem(string name)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (string.Equals(Items[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return Items[i];
                }
            }

            return null;
        }

        public KitDefinition Clone()
        {
            var copy = new KitDefinition
            {
                Id = Id,
                Name = Name,
                Title = Title,
                Description = Description,
                Image = Image,
                ImageAtlas = ImageAtlas,
                AutoStats = AutoStats
            };

            foreach (var p in Progressions)
            {
                copy.Progressions.Add(new KitProgression { Name = p.Name, Level = p.Level });
            }

            foreach (var item in Items)
            {
                copy.Items.Add(new KitItem { Name = item.Name, Count = item.Count, Quality = item.Quality });
            }

            foreach (var cvar in Cvars)
            {
                copy.Cvars.Add(new KitCvar { Name = cvar.Name, Value = cvar.Value });
            }

            foreach (var stat in Stats)
            {
                copy.Stats.Add(new KitStat { Text = stat.Text, Icon = stat.Icon, Atlas = stat.Atlas, Color = stat.Color });
            }

            return copy;
        }
    }

    public sealed class KitProgression
    {
        public string Name;
        public int Level;
    }

    public sealed class KitItem
    {
        public string Name;
        public int Count = 1;

        /// <summary>1-6 for items with quality tiers; 0 means "game default".</summary>
        public int Quality;
    }

    public sealed class KitCvar
    {
        public string Name;
        public float Value;
    }

    public sealed class KitStat
    {
        public string Text;
        public string Icon;
        public string Atlas = KitAtlases.Ui;
        public string Color;
    }

    public static class KitAtlases
    {
        public const string Ui = "UIAtlas";
        public const string ItemIcons = "ItemIconAtlas";

        /// <summary>Not an atlas: a PNG in Kits/Images imported with the kit editor (see KitImageStore).</summary>
        public const string File = "File";
    }
}
