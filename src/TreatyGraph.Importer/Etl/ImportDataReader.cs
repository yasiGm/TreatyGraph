using TreatyGraph.Core.Etl;
using TreatyGraph.Core.Etl.Countries;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Importer.Etl;

/// <summary>Runs every rule in <see cref="RuleCatalog"/> against the raw files.</summary>
public static class ImportDataReader
{
    public static ImportData Read(SourceFiles files)
    {
        var countries = CountryLoader.Load(files.StateList);

        // Some source codes (e.g. ATOP members) are not COW states; the schema has foreign keys,
        // so rows that reference them are dropped here and reported.
        var events = KnownOnly("pair events", Run(RuleCatalog.DyadEvents, files, countries),
            e => countries.Contains(e.CcodeLo) && countries.Contains(e.CcodeHi));
        var countryWars = KnownOnly("war participations", Run(RuleCatalog.CountryWars, files, countries), w => countries.Contains(w.Ccode));
        var civilWars = KnownOnly("civil-war rows", Run(RuleCatalog.CivilWars, files, countries), w => countries.Contains(w.Ccode));
        var leaders = KnownOnly("leaders", Run(RuleCatalog.Leaders, files, countries), l => countries.Contains(l.Ccode));

        return new ImportData(countries, events, countryWars, civilWars, leaders);
    }

    private static List<T> Run<T>(IEnumerable<IEtlRule<T>> rules, SourceFiles files, CountryDirectory countries) =>
        rules.SelectMany(rule => rule.Load(files, countries)).ToList();

    private static List<T> KnownOnly<T>(string label, List<T> rows, Func<T, bool> isKnown)
    {
        var kept = rows.Where(isKnown).ToList();
        var skipped = rows.Count - kept.Count;
        if (skipped > 0)
        {
            Console.WriteLine($"     note: skipped {skipped} {label} involving country codes that are not in the COW state list.");
        }

        return kept;
    }
}
