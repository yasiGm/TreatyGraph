namespace TreatyGraph.Api.Data;

/// <summary>JSON returned to the browser (property names become camelCase).</summary>
public sealed record CountryMetaDto(int C, string Abbr, string Name, int[][] Spans, bool Leaders);
