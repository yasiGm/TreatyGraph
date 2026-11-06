using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Wars;

/// <summary>UCDP/PRIO inter-state conflicts (2008 on) -> per-country profile rows.</summary>
public sealed class UcdpCountryWarRule : IEtlRule<CountryWarRow>
{
    public IReadOnlyList<CountryWarRow> Load(SourceFiles files, CountryDirectory countries) =>
        FromRows(CsvReader.ReadDictionaries(files.UcdpConflicts), countries);

    public List<CountryWarRow> FromRows(IEnumerable<Dictionary<string, string>> rows, CountryDirectory countries)
    {
        var result = new List<CountryWarRow>();
        foreach (var c in UcdpConflictReader.FromRows(rows, countries))
        {
            foreach (var p in c.Periods)
            {
                foreach (var (mine, theirs) in new[] { (c.SideA, c.SideB), (c.SideB, c.SideA) })
                {
                    var opponents = theirs.Select(countries.Name).ToList();
                    foreach (var code in mine)
                    {
                        var allies = mine.Where(x => x != code).Select(countries.Name).ToList();
                        result.Add(new CountryWarRow(
                            code, c.Title, new DatePart(p.StartYear, null, null), new DatePart(p.EndYear, null, null),
                            null, false, opponents, allies, null));
                    }
                }
            }
        }

        return result;
    }
}
