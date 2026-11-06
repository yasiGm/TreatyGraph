using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Countries;

/// <summary>
/// Loads the COW State System Membership list (statelist2024.csv). Not an <see cref="IEtlRule{T}"/>:
/// every rule needs the resulting <see cref="CountryDirectory"/>, so this runs first.
/// </summary>
public static class CountryLoader
{
    public static CountryDirectory Load(string path) => FromRows(CsvReader.ReadDictionaries(path));

    /// <summary>
    /// The COW state list when it is there; otherwise the Gleditsch and Ward list (codes translated to COW codes),
    /// so the site does not depend on a single source for which countries exist.
    /// </summary>
    public static CountryDirectory Load(SourceFiles files)
    {
        var year = DateTime.UtcNow.Year;
        CountryDirectory states;
        if (File.Exists(files.StateList))
        {
            states = Load(files.StateList);
        }
        else if (files.GleditschWardStates.Any(File.Exists))
        {
            Console.WriteLine("  COW state list not found; using the Gleditsch and Ward list instead.");
            states = GleditschWard.Load(files.GleditschWardStates, year);
        }
        else
        {
            throw new FileNotFoundException("No country list found (COW statelist2024.csv or Gleditsch-Ward iisystem.dat).", files.StateList);
        }

        return WithExtraTerritories(states, year);
    }

    /// <summary>Adds Palestine, Vatican City and the territories in <see cref="ExtraTerritories"/>.</summary>
    public static CountryDirectory WithExtraTerritories(CountryDirectory states, int currentYear) =>
        new(states.All.Concat(ExtraTerritories.All(currentYear).Where(t => !states.Contains(t.Ccode))));

    public static CountryDirectory FromRows(IEnumerable<Dictionary<string, string>> rows)
    {
        var order = new List<int>();
        var abbr = new Dictionary<int, string>();
        var names = new Dictionary<int, string>();
        var spans = new Dictionary<int, List<YearSpan>>();

        foreach (var r in rows)
        {
            var code = Num.ParseInt(r["ccode"]);
            var start = Num.ParseInt(r["styear"]);
            var end = Num.ParseInt(r["endyear"]);
            if (code is null || start is null || end is null)
            {
                continue;
            }

            if (!spans.ContainsKey(code.Value))
            {
                order.Add(code.Value);
                abbr[code.Value] = r["stateabb"].Trim();
                names[code.Value] = r["statenme"].Trim();
                spans[code.Value] = new List<YearSpan>();
            }

            spans[code.Value].Add(new YearSpan(start.Value, end.Value));
        }

        return new CountryDirectory(order.Select(c => new Country(c, abbr[c], names[c], spans[c])));
    }
}
