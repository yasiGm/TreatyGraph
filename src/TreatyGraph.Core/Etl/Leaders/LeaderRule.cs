using System.Globalization;
using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Leaders;

/// <summary>Loads the Archigos extract (columns: ccode,leader,start,end,exit).</summary>
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
            list.Add(new Leader(code.Value, r["leader"].Trim(), start, end, r["exit"].Trim()));
        }

        return list;
    }
}
