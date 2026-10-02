namespace TreatyGraph.Api.Data;

/// <summary>JSON returned to the browser (property names become camelCase).</summary>
public sealed record EventDto(string T, string Src, string S, string? E, bool On, bool Lc, int K, string Title, string Detail);
