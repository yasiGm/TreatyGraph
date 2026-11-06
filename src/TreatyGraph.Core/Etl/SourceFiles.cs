namespace TreatyGraph.Core.Etl;

/// <summary>Where each raw dataset lives under the data folder (see README, "Data files").</summary>
public sealed record SourceFiles(string DataDir)
{
    public string StateList => Raw("cow_states", "statelist2024.csv");
    public IReadOnlyList<string> GleditschWardStates => new[]
    {
        FirstExisting(Raw("gleditsch_ward", "iisystem.dat"), Raw("gleditsch_ward", "iisystem.dat.txt")),
        FirstExisting(Raw("gleditsch_ward", "microstatessystem.dat"), Raw("gleditsch_ward", "microstatessystem.dat.txt")),
    };
    public string CowAlliances => Raw("cow_alliances", "alliance_v4.1_by_dyad.csv");
    public string AtopMembers => Raw("atop", "atop5_1m.csv");
    public string AtopAlliances => Raw("atop", "atop5_1a.csv");
    public string InterStateWars => Raw("cow_wars", "Inter-StateWarData_v4.0.csv");
    public string IntraStateWars => Raw("cow_wars", "Intra-StateWarData_v4.1.csv");
    public string UcdpConflicts => Raw("ucdp", "UcdpPrioConflict_v26_1.csv");
    /// <summary>The newest UCDP Candidate Events file (GEDEvent_v*.csv) in data/raw/ucdp, or null when there is none.</summary>
    public string? UcdpCandidate
    {
        get
        {
            var dir = Path.Combine(DataDir, "raw", "ucdp");
            return Directory.Exists(dir)
                ? Directory.GetFiles(dir, "GEDEvent_v*.csv").OrderBy(f => f, StringComparer.Ordinal).LastOrDefault()
                : null;
        }
    }

    public string ExternalSupport => Raw("ucdp_esd", "esd_triads.csv");
    public string Diplomacy => Raw("cow_diplomacy", "Diplomatic_Exchange_2006v1.csv");
    public string WikidataDiplomacy => Raw("wikidata_diplomacy", "diplomatic_relations.csv");
    public string Leaders => Raw("leaders", "leaders_combined.csv");

    private static string FirstExisting(string a, string b) => File.Exists(a) ? a : b;

    private string Raw(string folder, string file) => Path.Combine(DataDir, "raw", folder, file);
}
