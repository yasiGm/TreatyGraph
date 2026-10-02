using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Diplomacy;

/// <summary>
/// COW Diplomatic Exchange v2006.1. The file has one row per country pair per snapshot year
/// (about every 5 years). We keep only the moments where the coded level of representation changes.
/// </summary>
public sealed class DiplomacyRule : IEtlRule<DyadEvent>
{
    public const string Source = "COW Diplomatic Exchange v2006.1";

    private static readonly Dictionary<int, string> Levels = new()
    {
        [0] = "no representation recorded",
        [1] = "chargé d'affaires",
        [2] = "minister",
        [3] = "ambassador",
        [9] = "other",
    };

    // The file is large, so it is read line by line instead of through CsvReader (it has no quoted commas).
    public IReadOnlyList<DyadEvent> Load(SourceFiles files, CountryDirectory countries)
    {
        var path = files.Diplomacy;
        var rows = new Dictionary<(int, int), List<(int Year, int L1, int L2)>>();

        using var reader = new StreamReader(path);
        var header = reader.ReadLine() ?? throw new InvalidDataException("Empty diplomacy file: " + path);
        var cols = header.Split(',').Select(h => h.Trim().Trim('"')).ToList();
        int Index(string name)
        {
            var i = cols.IndexOf(name);
            return i >= 0 ? i : throw new InvalidDataException($"Column '{name}' not found in {path}");
        }

        int iA = Index("ccode1"), iB = Index("ccode2"), iYear = Index("year"), iL1 = Index("DR_at_1"), iL2 = Index("DR_at_2");

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (line.Length == 0)
            {
                continue;
            }

            var f = line.Split(',');
            var a = int.Parse(f[iA].Trim('"'));
            var b = int.Parse(f[iB].Trim('"'));
            if (a >= b)
            {
                continue; // every pair is listed in both directions; keep one
            }

            var key = (a, b);
            if (!rows.TryGetValue(key, out var list))
            {
                list = new List<(int, int, int)>();
                rows[key] = list;
            }

            list.Add((int.Parse(f[iYear].Trim('"')), int.Parse(f[iL1].Trim('"')), int.Parse(f[iL2].Trim('"'))));
        }

        return FromRows(rows, countries);
    }

    public List<DyadEvent> FromRows(
        IReadOnlyDictionary<(int, int), List<(int Year, int L1, int L2)>> rows,
        CountryDirectory countries)
    {
        var events = new List<DyadEvent>();

        foreach (var (key, list) in rows)
        {
            var (a, b) = key;
            (int L1, int L2)? previous = null;
            int? previousYear = null;

            foreach (var (year, l1, l2) in list.OrderBy(x => x.Year))
            {
                var current = (l1, l2);
                var changed = previous is null || previous.Value != current;
                var initialZero = previous is null && current == (0, 0);

                if (changed && !initialZero)
                {
                    var title = current == (0, 0)
                        ? "No diplomatic representation recorded"
                        : $"Diplomatic representation: {countries.Name(a)}: {Level(l1)} · {countries.Name(b)}: {Level(l2)}";

                    var detail = "COW diplomatic exchange snapshot (data exist only every ~5 years).";
                    if (previousYear is not null)
                    {
                        detail += $" The change happened some time after the {previousYear} snapshot.";
                    }

                    if (year >= 1950 && year <= 1965 && current != (0, 0))
                    {
                        detail += " 1950–1965 snapshots are all coded 'other', so specific levels are unavailable.";
                    }

                    events.Add(DyadEvent.Create(a, b, EventTypes.Diplomacy, Source, new DatePart(year, null, null), null, title, detail));
                }

                previous = current;
                previousYear = year;
            }
        }

        return events;
    }

    private static string Level(int code) => Levels.TryGetValue(code, out var s) ? s : $"level {code}";
}
