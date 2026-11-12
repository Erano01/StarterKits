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
    }
}
