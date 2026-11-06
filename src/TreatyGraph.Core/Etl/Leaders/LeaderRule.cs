using System.Globalization;
using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Leaders;

/// <summary>
/// Loads the combined leaders file (columns: ccode,leader,start,end,exit and optionally role,source).
/// Without the optional columns a row counts as an Archigos "both" row.
/// </summary>
public sealed class LeaderRule : IEtlRule<Leader>
{
    public IReadOnlyList<Leader> Load(SourceFiles files, CountryDirectory countries) =>
        FromRows(CsvReader.ReadDictionaries(files.Leaders));

    public List<Leader> FromRows(IEnumerable<Dictionary<string, string>> rows)
    {
        var list = new List<Leader>();
        foreach (var r in rows)
        {
            var code = Num.ParseInt(r["ccode"]);
            if (code is null)
            {
                continue;
            }

            var start = DateOnly.ParseExact(r["start"].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var end = DateOnly.ParseExact(r["end"].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var role = r.Get("role").Trim();
            var source = r.Get("source").Trim();
            list.Add(new Leader(
                code.Value,
                r["leader"].Trim(),
                start,
                end,
                r["exit"].Trim(),
                role.Length > 0 ? role : "both",
                source.Length > 0 ? source : "Archigos 4.1"));
        }

        return list;
    }
}
