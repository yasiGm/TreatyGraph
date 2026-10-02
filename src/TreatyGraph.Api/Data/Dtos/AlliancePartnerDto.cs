namespace TreatyGraph.Api.Data;

/// <summary>JSON returned to the browser (property names become camelCase).</summary>
public sealed record AlliancePartnerDto(int Partner, string PartnerName, IReadOnlyList<AllianceItemDto> Items);
