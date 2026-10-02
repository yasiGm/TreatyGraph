namespace TreatyGraph.Api.Data;

/// <summary>JSON returned to the browser (property names become camelCase).</summary>
public sealed record LeaderDto(string Name, string S, string E, string Exit);
