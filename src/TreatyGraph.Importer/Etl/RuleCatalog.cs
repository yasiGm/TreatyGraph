using TreatyGraph.Core.Etl;
using TreatyGraph.Core.Etl.Alliances;
using TreatyGraph.Core.Etl.Diplomacy;
using TreatyGraph.Core.Etl.Leaders;
using TreatyGraph.Core.Etl.Wars;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Importer.Etl;

/// <summary>
/// Every ETL rule the importer runs, grouped by what it produces. To add a source, write a new
/// <see cref="IEtlRule{T}"/> class in TreatyGraph.Core and add one line here.
/// Dyad events are inserted in this order (it breaks ties in the timeline ordering).
/// </summary>
public static class RuleCatalog
{
    public static readonly IReadOnlyList<IEtlRule<DyadEvent>> DyadEvents = new IEtlRule<DyadEvent>[]
    {
        new CowAllianceRule(),
        new AtopAllianceRule(),
        new InterStateWarRule(),
        new DiplomacyRule(),
    };

    public static readonly IReadOnlyList<IEtlRule<CountryWarRow>> CountryWars = new IEtlRule<CountryWarRow>[]
    {
        new CountryWarRule(),
    };

    public static readonly IReadOnlyList<IEtlRule<CivilWarRow>> CivilWars = new IEtlRule<CivilWarRow>[]
    {
        new CivilWarRule(),
    };

    public static readonly IReadOnlyList<IEtlRule<Leader>> Leaders = new IEtlRule<Leader>[]
    {
        new LeaderRule(),
    };
}
