namespace TreatyGraph.Core.Model;

/// <summary>One continuous span of years in which a country was a COW state-system member.</summary>
public readonly record struct YearSpan(int Start, int End);
