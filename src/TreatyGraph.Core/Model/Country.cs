namespace TreatyGraph.Core.Model;

/// <summary>One Correlates of War country code, with the year span(s) it was a state-system member.</summary>
public sealed record Country(int Ccode, string Abbr, string Name, IReadOnlyList<YearSpan> Spans);
