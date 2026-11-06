namespace TreatyGraph.Core.Model;

/// <summary>
/// One term in office. <paramref name="Role"/> is "state" (head of state), "government" (head of government)
/// or "both" (one office that is both, or one combined list); <paramref name="Source"/> says where it came from.
/// </summary>
public sealed record Leader(
    int Ccode,
    string Name,
    DateOnly Start,
    DateOnly End,
    string ExitType,
    string Role = "both",
    string Source = "Archigos 4.1");
