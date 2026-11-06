using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Etl.Countries;
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
/// inter-state conflicts (type_of_conflict = 2) from <c>firstYear</c> on (default <see cref="FirstYear"/>,
/// because the COW inter-state war data already covers the earlier years). UCDP uses Gleditsch-Ward country codes,
/// translated to COW codes by <see cref="GleditschWard"/>; unknown codes are dropped.
/// </summary>
public static class UcdpConflictReader
{
    public const int FirstYear = 2008;

    public static List<UcdpConflict> FromRows(IEnumerable<Dictionary<string, string>> rows, CountryDirectory countries, int firstYear = FirstYear)
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
            var sideA = Codes(g.Select(x => (x.Row.Get("gwno_a"), x.Year!.Value)), countries);
            var sideB = Codes(g.Select(x => (x.Row.Get("gwno_b"), x.Year!.Value)), countries);
            if (sideA.Count == 0 || sideB.Count == 0)
            {
                continue;
            }

            var years = g.Where(x => x.Year >= firstYear)
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

    // UCDP cells hold Gleditsch-Ward codes, sometimes several separated by commas; each is turned into a COW code.
    private static List<int> Codes(IEnumerable<(string Cell, int Year)> cells, CountryDirectory countries) =>
        cells.SelectMany(c => c.Cell.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => Num.ParseInt(s))
                .Where(code => code is not null)
                .Select(code => GleditschWard.ToCow(code!.Value, c.Year)))
            .Where(countries.Contains)
            .Distinct()
            .ToList();
}
