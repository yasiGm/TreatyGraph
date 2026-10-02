using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Alliances;

/// <summary>COW Formal Alliances v4.1 (dyad level) -> one alliance event per row.</summary>
public sealed class CowAllianceRule : IEtlRule<DyadEvent>
{
    public const string Source = "COW Alliances v4.1";

    public IReadOnlyList<DyadEvent> Load(SourceFiles files, CountryDirectory countries) =>
        FromRows(CsvReader.ReadDictionaries(files.CowAlliances));

    public List<DyadEvent> FromRows(IEnumerable<Dictionary<string, string>> rows)
    {
        var events = new List<DyadEvent>();
        foreach (var r in rows)
        {
            var a = Num.ParseInt(r.Get("ccode1"));
            var b = Num.ParseInt(r.Get("ccode2"));
            var start = DatePart.Clean(r.Get("dyad_st_year"), r.Get("dyad_st_month"), r.Get("dyad_st_day"));
            var end = DatePart.Clean(r.Get("dyad_end_year"), r.Get("dyad_end_month"), r.Get("dyad_end_day"));
            if (a is null || b is null || start is null)
            {
                continue;
            }

            var flags = new List<string>();
            foreach (var name in new[] { "defense", "neutrality", "nonaggression", "entente" })
            {
                if (Num.ParseInt(r.Get(name)) == 1)
                {
                    flags.Add(name);
                }
            }

            var detail = $"COW formal alliance #{r.Get("version4id")}. Commitments coded: "
                         + (flags.Count > 0 ? string.Join(", ", flags) : "n/a") + ".";
            if (Num.ParseInt(r.Get("asymmetric")) == 1)
            {
                detail += " Asymmetric (obligations differ between the parties).";
            }

            var ongoing = end is null && Num.ParseInt(r.Get("right_censor")) == 1;
            var leftCensored = Num.ParseInt(r.Get("left_censor")) == 1;
            events.Add(DyadEvent.Create(a.Value, b.Value, EventTypes.Alliance, Source, start.Value, end, Title(flags), detail, ongoing, leftCensored));
        }

        return events;
    }

    private static string Title(List<string> flags)
    {
        if (flags.Contains("defense"))
        {
            return "Defense pact";
        }

        if (flags.Contains("neutrality") && flags.Contains("nonaggression"))
        {
            return "Neutrality / non-aggression treaty";
        }

        if (flags.Contains("neutrality"))
        {
            return "Neutrality pact";
        }

        if (flags.Contains("nonaggression"))
        {
            return "Non-aggression pact";
        }

        return "Entente";
    }
}
