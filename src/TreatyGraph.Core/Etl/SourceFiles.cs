namespace TreatyGraph.Core.Etl;

/// <summary>Where each raw dataset lives under the data folder (see README, "Data files").</summary>
public sealed record SourceFiles(string DataDir)
{
    public string StateList => Raw("cow_states", "statelist2024.csv");
    public string CowAlliances => Raw("cow_alliances", "alliance_v4.1_by_dyad.csv");
    public string AtopMembers => Raw("atop", "atop5_1m.csv");
    public string AtopAlliances => Raw("atop", "atop5_1a.csv");
    public string InterStateWars => Raw("cow_wars", "Inter-StateWarData_v4.0.csv");
    public string IntraStateWars => Raw("cow_wars", "Intra-StateWarData_v4.1.csv");
    public string Diplomacy => Raw("cow_diplomacy", "Diplomatic_Exchange_2006v1.csv");
    public string Leaders => Path.Combine(DataDir, "leaders_pilot.csv");

    private string Raw(string folder, string file) => Path.Combine(DataDir, "raw", folder, file);
}
