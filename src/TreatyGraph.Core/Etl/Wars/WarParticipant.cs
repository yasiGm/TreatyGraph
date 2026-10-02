using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Wars;

/// <summary>One country's side, period(s) of participation, and outcome in an inter-state war.</summary>
public sealed record WarParticipant(
    int Ccode,
    string Side,
    IReadOnlyList<(DatePart Start, DatePart End)> Periods,
    int? Outcome,
    int? Initiator,
    int Deaths)
{
    private static readonly Dictionary<int, string> Outcomes = new()
    {
        [1] = "winner",
        [2] = "loser",
        [3] = "compromise",
        [6] = "stalemate",
    };

    /// <summary>winner | loser | compromise | stalemate, or null when not coded.</summary>
    public string? Result => Outcome is not null && Outcomes.TryGetValue(Outcome.Value, out var r) ? r : null;

    public bool IsInitiator => Initiator == 1;

    public int? BattleDeaths => Deaths > 0 ? Deaths : null;
}
