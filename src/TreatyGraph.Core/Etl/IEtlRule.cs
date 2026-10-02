using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl;

/// <summary>
/// One ETL rule: reads one source dataset and turns it into rows of type <typeparamref name="T"/>.
/// Adding a new source means one new class implementing this interface, registered in the
/// importer's RuleCatalog - nothing else has to change.
/// </summary>
public interface IEtlRule<T>
{
    IReadOnlyList<T> Load(SourceFiles files, CountryDirectory countries);
}
