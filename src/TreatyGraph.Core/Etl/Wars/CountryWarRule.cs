using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Wars;

/// <summary>COW Inter-State Wars -> per-country profile rows: who fought against / alongside whom.</summary>
public sealed class CountryWarRule : IEtlRule<CountryWarRow>
{
    public IReadOnlyList<CountryWarRow> Load(SourceFiles files, CountryDirectory countries) =>
        FromRows(CsvReader.ReadDictionaries(files.InterStateWars), countries);

    public List<CountryWarRow> FromRows(IEnumerable<Dictionary<string, string>> rows, CountryDirectory countries)
    {
        var result = new List<CountryWarRow>();

        foreach (var war in InterStateWarReader.FromRows(rows))
        {
            var parts = war.Participants;
            foreach (var p in parts)
            {
                var opponents = parts.Where(q => q.Side != p.Side).Select(q => countries.Name(q.Ccode)).ToList();
                var allies = parts.Where(q => q.Side == p.Side && q.Ccode != p.Ccode).Select(q => countries.Name(q.Ccode)).ToList();
                foreach (var (s, e) in p.Periods)
                {
                    result.Add(new CountryWarRow(p.Ccode, war.Title, s, e, p.Result, p.IsInitiator, opponents, allies, p.BattleDeaths));
                }
            }
        }

        return result;
    }
}
