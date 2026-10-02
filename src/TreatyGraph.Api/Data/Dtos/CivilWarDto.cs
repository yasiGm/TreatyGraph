namespace TreatyGraph.Api.Data;

/// <summary>JSON returned to the browser (property names become camelCase).</summary>
public sealed record CivilWarDto(string Name, string S, string? E, int K, string Sides, bool Internationalized);
