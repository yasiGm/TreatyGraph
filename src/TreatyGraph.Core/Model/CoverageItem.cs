namespace TreatyGraph.Core.Model;

/// <summary>One line of the "what is in the timeline" coverage table shown to the user.</summary>
public sealed record CoverageItem(string Src, int From, int To, string Note);
