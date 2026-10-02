using TreatyGraph.Core.Model;

namespace TreatyGraph.Importer.Etl;

/// <summary>Everything the rules extracted from the raw files; the input to every table class.</summary>
public sealed record ImportData(
    CountryDirectory Countries,
    IReadOnlyList<DyadEvent> DyadEvents,
    IReadOnlyList<CountryWarRow> CountryWars,
    IReadOnlyList<CivilWarRow> CivilWars,
    IReadOnlyList<Leader> Leaders);
