using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Wars;

/// <summary>
/// UCDP/PRIO inter-state conflicts -> pair events: "war" for states on opposing sides,
/// "coalition" for states on the same side.
/// From 2008 on UCDP continues the COW war data, which ends in 2007. For 1946-2007 UCDP is used as a
/// second source: a pair's conflict is kept only when COW has no war for that pair in those years,
/// so smaller conflicts (25+ deaths a year, below COW's 1,000-death threshold) appear without doubling the big wars.
/// </summary>
public sealed class UcdpInterStateRule : IEtlRule<DyadEvent>
{
    public const string Source = "UCDP/PRIO Armed Conflict v26.1";
    public const int EarliestYear = 1946;

    public IReadOnlyList<DyadEvent> Load(SourceFiles files, CountryDirectory countries)
    {
        var cow = CowPairYears(InterStateWarReader.FromRows(CsvReader.ReadDictionaries(files.InterStateWars)));
        return FromRows(CsvReader.ReadDictionaries(files.UcdpConflicts), countries, EarliestYear, cow);
    }

    /// <summary>Every (pair, year) in which two countries took part in the same COW inter-state war.</summary>
    public static HashSet<(int Lo, int Hi, int Year)> CowPairYears(IEnumerable<InterStateWar> wars)
    {
        var set = new HashSet<(int, int, int)>();
        foreach (var war in wars)
        {
            var ps = war.Participants;
            for (var i = 0; i < ps.Count; i++)
            {
                for (var j = i + 1; j < ps.Count; j++)
                {
                    if (ps[i].Ccode == ps[j].Ccode)
                    {
                        continue;
                    }

                    var lo = Math.Min(ps[i].Ccode, ps[j].Ccode);
                    var hi = Math.Max(ps[i].Ccode, ps[j].Ccode);
                    foreach (var (s1, e1) in ps[i].Periods)
                    {
                        foreach (var (s2, e2) in ps[j].Periods)
                        {
                            for (var y = Math.Max(s1.Year, s2.Year); y <= Math.Min(e1.Year, e2.Year); y++)
                            {
                                set.Add((lo, hi, y));
                            }
                        }
                    }
                }
            }
        }

        return set;
    }

    public List<DyadEvent> FromRows(IEnumerable<Dictionary<string, string>> rows, CountryDirectory countries) =>
        FromRows(rows, countries, UcdpConflictReader.FirstYear, new HashSet<(int, int, int)>());

    public List<DyadEvent> FromRows(
        IEnumerable<Dictionary<string, string>> rows,
        CountryDirectory countries,
        int firstYear,
        HashSet<(int Lo, int Hi, int Year)> cowPairYears)
    {
        var events = new List<DyadEvent>();
        foreach (var c in UcdpConflictReader.FromRows(rows, countries, firstYear))
        {
            foreach (var p in c.Periods)
            {
                var kind = p.MaxIntensity >= 2 ? "war (1,000+ deaths a year)" : "armed conflict (25-999 deaths a year)";
                var detail = $"UCDP conflict #{c.ConflictId}, {kind}.";
                if (p.StartYear < UcdpConflictReader.FirstYear)
                {
                    detail += " Not in the COW war list for this pair in these years.";
                }

                var start = new DatePart(p.StartYear, null, null);
                var end = new DatePart(p.EndYear, null, null);

                // Before 2008 COW is the main war source: skip a pair COW already lists as fighting in these years.
                bool InCow(int a, int b)
                {
                    if (p.StartYear >= UcdpConflictReader.FirstYear)
                    {
                        return false;
                    }

                    var lo = Math.Min(a, b);
                    var hi = Math.Max(a, b);
                    for (var y = p.StartYear; y <= Math.Min(p.EndYear, UcdpConflictReader.FirstYear - 1); y++)
                    {
                        if (cowPairYears.Contains((lo, hi, y)))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                foreach (var a in c.SideA)
                {
                    foreach (var b in c.SideB.Where(b => b != a && !InCow(a, b)))
                    {
                        events.Add(DyadEvent.Create(a, b, EventTypes.War, Source, start, end, c.Title + " — on opposing sides", detail, p.IsLatestYear));
                    }
                }

                foreach (var side in new[] { c.SideA, c.SideB })
                {
                    for (var i = 0; i < side.Count; i++)
                    {
                        for (var j = i + 1; j < side.Count; j++)
                        {
                            if (InCow(side[i], side[j]))
                            {
                                continue;
                            }

                            events.Add(DyadEvent.Create(side[i], side[j], EventTypes.Coalition, Source, start, end, c.Title + " — on the same side", detail, p.IsLatestYear));
                        }
                    }
                }
            }
        }

        return events;
    }
}
