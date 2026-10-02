namespace TreatyGraph.Core.Model;

/// <summary>One country's participation in an inter-state war (used for country profiles).</summary>
public sealed record CountryWarRow(
    int Ccode,
    string Name,
    DatePart Start,
    DatePart End,
    string? Result,
    bool Initiator,
    IReadOnlyList<string> Opponents,
    IReadOnlyList<string> Allies,
    int? BattleDeaths);
