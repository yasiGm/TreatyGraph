using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Wars;

/// <summary>One inter-state conflict from UCDP/PRIO, with the years it was active.</summary>
public sealed record UcdpConflict(
    string ConflictId,
    string Title,
    IReadOnlyList<int> SideA,
    IReadOnlyList<int> SideB,
    IReadOnlyList<UcdpPeriod> Periods);

/// <summary>A run of consecutive active years of one conflict. Intensity 2 = war (1000+ deaths a year).</summary>
public sealed record UcdpPeriod(int StartYear, int EndYear, int MaxIntensity, bool IsLatestYear);

/// <summary>
/// Parses the UCDP/PRIO Armed Conflict Dataset (one row per conflict per year) and keeps only
/// inter-state conflicts (type_of_conflict = 2) from <see cref="FirstYear"/> on, because the COW
/// inter-state war data already covers the earlier years. UCDP uses Gleditsch-Ward country codes,
/// which match the COW codes for the states we use; unknown codes are dropped.
/// </summary>
public static class UcdpConflictReader
{
    public const int FirstYear = 2008;

    public static List<UcdpConflict> FromRows(IEnumerable<Dictionary<string, string>> rows, CountryDirectory countries)
    {
        var inter = rows
            .Where(r => Num.ParseInt(r.Get("type_of_conflict")) == 2)
            .Select(r => (Row: r, Year: Num.ParseInt(r.Get("year"))))
            .Where(x => x.Year is not null)
            .ToList();
        var latest = inter.Count == 0 ? 0 : inter.Max(x => x.Year!.Value);

        var result = new List<UcdpConflict>();
        foreach (var g in inter.GroupBy(x => x.Row.Get("conflict_id")))
        {
            var first = g.First().Row;
            var sideA = Codes(g.Select(x => x.Row.Get("gwno_a")), countries);
            var sideB = Codes(g.Select(x => x.Row.Get("gwno_b")), countries);
            if (sideA.Count == 0 || sideB.Count == 0)
            {
                continue;
            }

            var years = g.Where(x => x.Year >= FirstYear)
                .GroupBy(x => x.Year!.Value)
                .ToDictionary(y => y.Key, y => y.Max(x => Num.ParseInt(x.Row.Get("intensity_level")) ?? 1));

            var periods = new List<UcdpPeriod>();
            foreach (var y in years.Keys.OrderBy(k => k))
            {
                var last = periods.Count > 0 ? periods[^1] : null;
                if (last is not null && last.EndYear == y - 1)
                {
                    periods[^1] = last with { EndYear = y, MaxIntensity = Math.Max(last.MaxIntensity, years[y]), IsLatestYear = y == latest };
                }
                else
                {
                    periods.Add(new UcdpPeriod(y, y, years[y], y == latest));
                }
            }

            if (periods.Count > 0)
            {
                result.Add(new UcdpConflict(g.Key, first.Get("location").Trim() + " conflict", sideA, sideB, periods));
            }
        }

        return result;
    }

    private static List<int> Codes(IEnumerable<string> cells, CountryDirectory countries) =>
        cells.SelectMany(c => c.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(s => Num.ParseInt(s))
            .Where(c => c is not null && countries.Contains(c.Value))
            .Select(c => c!.Value)
            .Distinct()
            .ToList();
}
