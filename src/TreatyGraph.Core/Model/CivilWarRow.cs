namespace TreatyGraph.Core.Model;

/// <summary>One country's participation in a civil / internationalised war.</summary>
public sealed record CivilWarRow(
    int Ccode,
    string Name,
    DatePart Start,
    DatePart? End,
    string Sides,
    bool Internationalized);
