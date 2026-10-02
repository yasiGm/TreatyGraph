namespace TreatyGraph.Api.Data;

/// <summary>JSON returned to the browser (property names become camelCase).</summary>
public sealed record CountryProfileDto(
    int C,
    string Abbr,
    string Name,
    int[][] Spans,
    IReadOnlyList<LeaderDto>? Leaders,
    IReadOnlyList<WarDto> Wars,
    IReadOnlyList<CivilWarDto> Civil,
    IReadOnlyList<AlliancePartnerDto> Alliances);
