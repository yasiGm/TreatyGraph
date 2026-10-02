namespace TreatyGraph.Core.Etl.Wars;

/// <summary>One war from the COW Inter-State War file, with all of its participants.</summary>
public sealed record InterStateWar(string WarNum, string Title, IReadOnlyList<WarParticipant> Participants);
