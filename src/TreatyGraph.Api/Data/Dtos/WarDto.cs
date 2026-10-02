namespace TreatyGraph.Api.Data;

/// <summary>JSON returned to the browser (property names become camelCase).</summary>
public sealed record WarDto(string Name, string S, string E, int K, string? Result, bool Initiator, string[] Vs, string[] With, int? Deaths);
