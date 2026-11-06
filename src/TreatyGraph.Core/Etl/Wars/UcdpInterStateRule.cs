using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Wars;

/// <summary>
/// UCDP/PRIO inter-state conflicts (2008 on) -> pair events: "war" for states on opposing sides,
/// "coalition" for states on the same side. Extends the COW war data, which ends in 2007.
/// </summary>
public sealed class UcdpInterStateRule : IEtlRule<DyadEvent>
{
    public const string Source = "UCDP/PRIO Armed Conflict v26.1";

    public IReadOnlyList<DyadEvent> Load(SourceFiles files, CountryDirectory countries) =>
        FromRows(CsvReader.ReadDictionaries(files.UcdpConflicts), countries);

    public List<DyadEvent> FromRows(IEnumerable<Dictionary<string, string>> rows, CountryDirectory countries)
    {
        var events = new List<DyadEvent>();
        foreach (var c in UcdpConflictReader.FromRows(rows, countries))
        {
            foreach (var p in c.Periods)
            {
                var kind = p.MaxIntensity >= 2 ? "war (1,000+ deaths a year)" : "armed conflict (25-999 deaths a year)";
                var detail = $"UCDP conflict #{c.ConflictId}, {kind}.";
                var start = new DatePart(p.StartYear, null, null);
                var end = new DatePart(p.EndYear, null, null);

                foreach (var a in c.SideA)
                {
                    foreach (var b in c.SideB.Where(b => b != a))
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
                            events.Add(DyadEvent.Create(side[i], side[j], EventTypes.Coalition, Source, start, end, c.Title + " — on the same side", detail, p.IsLatestYear));
                        }
                    }
                }
            }
        }

        return events;
    }
}
