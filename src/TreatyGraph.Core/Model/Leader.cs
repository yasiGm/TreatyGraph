namespace TreatyGraph.Core.Model;

/// <summary>One leader's term in office (Archigos 4.1), for all countries covered by Archigos.</summary>
public sealed record Leader(int Ccode, string Name, DateOnly Start, DateOnly End, string ExitType);
