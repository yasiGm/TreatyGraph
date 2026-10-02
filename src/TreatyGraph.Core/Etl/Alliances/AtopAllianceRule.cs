using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Alliances;

/// <summary>
/// ATOP 5.1. Member-level entry/exit dates are turned into pair windows (a pair is allied from the
/// later entry until the earlier exit), and consecutive phases of one alliance are merged.
/// </summary>
public sealed class AtopAllianceRule : IEtlRule<DyadEvent>
{
    public const string Source = "ATOP 5.1";

    private sealed record Membership(int Ccode, DatePart Start, DatePart? End);

    public IReadOnlyList<DyadEvent> Load(SourceFiles files, CountryDirectory countries) =>
        FromRows(CsvReader.ReadDictionaries(files.AtopMembers), CsvReader.ReadDictionaries(files.AtopAlliances));

    public List<DyadEvent> FromRows(
        IEnumerable<Dictionary<string, string>> memberRows,
        IEnumerable<Dictionary<string, string>> allianceRows)
    {
        var attributes = new Dictionary<string, Dictionary<string, string>>();
        foreach (var r in allianceRows)
        {
            attributes[r.Get("atopid")] = r;
        }

        var events = new List<DyadEvent>();
        foreach (var (atopId, rows) in Memberships(memberRows))
        {
            attributes.TryGetValue(atopId, out var attr);
            var flags = Flags(attr ?? new Dictionary<string, string>());
            var title = Title(flags);
            var memberCount = rows.Select(m => m.Ccode).Distinct().Count();

            foreach (var (pair, windows) in PairWindows(rows))
            {
                foreach (var (start, end) in Merge(windows))
                {
                    var detail = $"ATOP alliance #{atopId} ({memberCount} members). Obligations coded: "
                                 + (flags.Count > 0 ? string.Join(", ", flags) : "n/a") + ".";
                    events.Add(DyadEvent.Create(pair.Lo, pair.Hi, EventTypes.Alliance, Source, start, end, title, detail, ongoing: end is null));
                }
            }
        }

        return events;
    }

    /// <summary>atopid -> memberships (one row per member per phase).</summary>
    private static Dictionary<string, List<Membership>> Memberships(IEnumerable<Dictionary<string, string>> memberRows)
    {
        var members = new Dictionary<string, List<Membership>>();
        foreach (var r in memberRows)
        {
            var code = Num.ParseInt(r.Get("member"));
            var start = AtopDate(r.Get("yrent"), r.Get("moent"), r.Get("dayent"));
            var end = AtopDate(r.Get("yrexit"), r.Get("moexit"), r.Get("dayexit"));
            if (code is null || code == 0 || start is null)
            {
                continue;
            }

            var id = r.Get("atopid");
            if (!members.TryGetValue(id, out var list))
            {
                list = new List<Membership>();
                members[id] = list;
            }

            list.Add(new Membership(code.Value, start.Value, end));
        }

        return members;
    }

    private static List<string> Flags(Dictionary<string, string> attr)
    {
        var flags = new List<string>();
        foreach (var (column, label) in new[]
                 {
                     ("defense", "defense"), ("offense", "offense"), ("neutral", "neutrality"),
                     ("nonagg", "non-aggression"), ("consul", "consultation"),
                 })
        {
            if ((Num.ParseInt(attr.Get(column)) ?? 0) > 0)
            {
                flags.Add(label);
            }
        }

        return flags;
    }

    private static string Title(List<string> flags)
    {
        if (flags.Contains("defense"))
        {
            return "Defense pact";
        }

        if (flags.Contains("offense"))
        {
            return "Offense pact";
        }

        if (flags.Contains("neutrality") && flags.Contains("non-aggression"))
        {
            return "Neutrality / non-aggression treaty";
        }

        if (flags.Contains("neutrality"))
        {
            return "Neutrality pledge";
        }

        if (flags.Contains("non-aggression"))
        {
            return "Non-aggression pact";
        }

        return "Consultation agreement";
    }

    private static Dictionary<(int Lo, int Hi), List<(DatePart Start, DatePart? End)>> PairWindows(List<Membership> rows)
    {
        var windows = new Dictionary<(int Lo, int Hi), List<(DatePart Start, DatePart? End)>>();

        for (var i = 0; i < rows.Count; i++)
        {
            for (var j = i + 1; j < rows.Count; j++)
            {
                var m1 = rows[i];
                var m2 = rows[j];
                if (m1.Ccode == m2.Ccode)
                {
                    continue;
                }

                // The pair is allied from the later entry until the earlier exit.
                var start = m1.Start.Key() >= m2.Start.Key() ? m1.Start : m2.Start;
                DatePart? end;
                if (m1.End is not null && m2.End is not null)
                {
                    end = m1.End.Value.Key(true) <= m2.End.Value.Key(true) ? m1.End : m2.End;
                }
                else
                {
                    end = m1.End ?? m2.End;
                }

                if (end is not null && end.Value.Key(true) < start.Key())
                {
                    continue;
                }

                var pair = (Math.Min(m1.Ccode, m2.Ccode), Math.Max(m1.Ccode, m2.Ccode));
                if (!windows.TryGetValue(pair, out var list))
                {
                    list = new List<(DatePart, DatePart?)>();
                    windows[pair] = list;
                }

                list.Add((start, end));
            }
        }

        return windows;
    }

    /// <summary>Overlapping / touching phases of the same alliance are merged.</summary>
    private static List<(DatePart Start, DatePart? End)> Merge(List<(DatePart Start, DatePart? End)> windows)
    {
        var merged = new List<(DatePart Start, DatePart? End)>();

        foreach (var (start, end) in windows.OrderBy(w => w.Start.Key()))
        {
            if (merged.Count > 0)
            {
                var last = merged[^1];
                if (last.End is null || start.Key() <= last.End.Value.Key(true) + 1)
                {
                    if (last.End is not null && (end is null || end.Value.Key(true) > last.End.Value.Key(true)))
                    {
                        merged[^1] = (last.Start, end);
                    }

                    continue;
                }
            }

            merged.Add((start, end));
        }

        return merged;
    }

    private static DatePart? AtopDate(string year, string month, string day)
    {
        var y = Num.ParseInt(year);
        if (y is null || y == 0)
        {
            return null; // 0 / blank exit year = alliance still in effect
        }

        return DatePart.FromInts(y, Num.ParseInt(month), Num.ParseInt(day));
    }
}
