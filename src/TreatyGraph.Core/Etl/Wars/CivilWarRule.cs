using System.Text;
using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Wars;

/// <summary>COW Intra-State War Data v4.1 -> one row per war per involved COW state.</summary>
public sealed class CivilWarRule : IEtlRule<CivilWarRow>
{
    public IReadOnlyList<CivilWarRow> Load(SourceFiles files, CountryDirectory countries) =>
        FromRows(CsvReader.ReadDictionaries(files.IntraStateWars, Encoding.Latin1), countries);

    public List<CivilWarRow> FromRows(IEnumerable<Dictionary<string, string>> rows, CountryDirectory countries)
    {
        var result = new List<CivilWarRow>();
        foreach (var r in rows)
        {
            var start = DatePart.Clean(r.Get("StartYear1"), r.Get("StartMonth1"), r.Get("StartDay1"));
            var end = DatePart.Clean(r.Get("EndYear1"), r.Get("EndMonth1"), r.Get("EndDay1"));
            if (start is null)
            {
                continue;
            }

            var sides = $"{SideName(r.Get("SideA"))} vs {SideName(r.Get("SideB"))}";
            var codes = new HashSet<int>();
            foreach (var key in new[] { "CcodeA", "CcodeB" })
            {
                var c = Num.ParseInt(r.Get(key));
                if (c is not null && c > 0 && countries.Contains(c.Value))
                {
                    codes.Add(c.Value);
                }
            }

            foreach (var c in codes)
            {
                result.Add(new CivilWarRow(c, r.Get("WarName").Trim(), start.Value, end, sides, Num.ParseInt(r.Get("Intnl")) == 1));
            }
        }

        return result;
    }

    private static string SideName(string raw)
    {
        var s = raw.Trim();
        return s is "" or "-8" or "-9" ? "other parties" : s;
    }
}
