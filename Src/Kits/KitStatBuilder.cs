using System;
using System.Collections.Generic;

namespace StarterKits.Kits
{
    /// <summary>
    /// Generates the "Kit Stats" lines (text + icon) from a kit's progressions and items.
    /// Book series are collapsed into one line, e.g. "Sniper 7/7", like the built-in kits do.
    /// </summary>
    public static class KitStatBuilder
    {
        /// <summary>The in-game overview shows at most this many lines.</summary>
        public const int MaxVisibleLines = 12;

        public static List<KitStat> Build(KitDefinition kit)
        {
            var lines = new List<KitStat>();
            var seriesDone = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (KitProgression p in kit.Progressions)
            {
                ProgressionInfo info = GameDataCatalog.FindProgression(p.Name);
                if (info == null)
                {
                    lines.Add(new KitStat { Text = $"{GameDataCatalog.Prettify(p.Name)} {p.Level}", Icon = "ui_game_symbol_modded" });
                    continue;
                }

                if (info.Category == ProgressionCategory.Book)
                {
                    ProgressionInfo series = GameDataCatalog.FindProgression(info.Parent);
                    if (series == null)
                    {
                        lines.Add(new KitStat { Text = info.DisplayName, Icon = info.Icon });
                        continue;
                    }

                    if (!seriesDone.Add(series.Name))
                    {
                        continue;
                    }

                    int owned = 0;
                    foreach (ProgressionInfo book in series.Books)
                    {
                        KitProgression granted = kit.FindProgression(book.Name);
                        if (granted != null && granted.Level > 0 && !GameDataCatalog.IsCompletionBook(book))
                        {
                            owned++;
                        }
                    }

                    lines.Add(new KitStat
                    {
                        Text = $"{series.DisplayName} {owned}/{GameDataCatalog.CountCollectibleBooks(series)}",
                        Icon = series.Icon
                    });
                    continue;
                }

                lines.Add(new KitStat { Text = $"{info.DisplayName} {p.Level}/{info.MaxLevel}", Icon = info.Icon });
            }

            foreach (KitItem item in kit.Items)
            {
                ItemInfo info = GameDataCatalog.FindItem(item.Name);
                string name = info != null ? info.DisplayName : item.Name;
                lines.Add(new KitStat
                {
                    Text = item.Count > 1 ? $"{name} x{item.Count}" : name,
                    Icon = info != null ? info.Icon : item.Name,
                    Atlas = KitAtlases.ItemIcons
                });
            }

            return lines;
        }

        /// <summary>
        /// Hand-written (Custom) stat lines are free text, so removing a progression from the kit
        /// would leave its line behind. Call after the progression was removed: drops the lines
        /// that start with its display name and re-counts book series lines ("Sniper 3/7").
        /// Returns how many lines were removed or changed.
        /// </summary>
        public static int SyncCustomStatsAfterRemoval(KitDefinition kit, string removedProgression)
        {
            ProgressionInfo info = GameDataCatalog.FindProgression(removedProgression);
            string name = info != null ? info.DisplayName : GameDataCatalog.Prettify(removedProgression);
            int changed = 0;

            if (info != null && info.Category == ProgressionCategory.Book)
            {
                ProgressionInfo series = GameDataCatalog.FindProgression(info.Parent);
                if (series != null)
                {
                    int owned = 0;
                    bool anyLeft = false;
                    foreach (ProgressionInfo book in series.Books)
                    {
                        KitProgression granted = kit.FindProgression(book.Name);
                        if (granted == null)
                        {
                            continue;
                        }

                        anyLeft = true;
                        if (granted.Level > 0 && !GameDataCatalog.IsCompletionBook(book))
                        {
                            owned++;
                        }
                    }

                    for (int i = kit.Stats.Count - 1; i >= 0; i--)
                    {
                        if (!StartsWithName(kit.Stats[i].Text, series.DisplayName))
                        {
                            continue;
                        }

                        if (anyLeft)
                        {
                            kit.Stats[i].Text = $"{series.DisplayName} {owned}/{GameDataCatalog.CountCollectibleBooks(series)}";
                        }
                        else
                        {
                            kit.Stats.RemoveAt(i);
                        }

                        changed++;
                    }
                }
            }

            for (int i = kit.Stats.Count - 1; i >= 0; i--)
            {
                if (StartsWithName(kit.Stats[i].Text, name))
                {
                    kit.Stats.RemoveAt(i);
                    changed++;
                }
            }

            return changed;
        }

        /// <summary>Same as <see cref="SyncCustomStatsAfterRemoval"/> for an item that left the kit.</summary>
        public static int SyncCustomStatsAfterItemRemoval(KitDefinition kit, string removedItem)
        {
            if (kit.FindItem(removedItem) != null)
            {
                return 0;
            }

            ItemInfo info = GameDataCatalog.FindItem(removedItem);
            string name = info != null ? info.DisplayName : removedItem;
            int changed = 0;
            for (int i = kit.Stats.Count - 1; i >= 0; i--)
            {
                if (StartsWithName(kit.Stats[i].Text, name))
                {
                    kit.Stats.RemoveAt(i);
                    changed++;
                }
            }

            return changed;
        }

        /// <summary>"Spear Master 5/5" starts with "Spear Master"; "Spear Masterpiece" does not.</summary>
        private static bool StartsWithName(string text, string name)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(name) ||
                !text.StartsWith(name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (text.Length == name.Length)
            {
                return true;
            }

            char next = text[name.Length];
            return char.IsWhiteSpace(next) || char.IsDigit(next) || next == ':' || next == '-' || next == '(';
        }
    }
}
