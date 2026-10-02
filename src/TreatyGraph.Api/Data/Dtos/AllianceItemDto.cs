namespace TreatyGraph.Api.Data;

/// <summary>JSON returned to the browser (property names become camelCase).</summary>
public sealed record AllianceItemDto(string Title, string Src, string S, string? E, bool On);
