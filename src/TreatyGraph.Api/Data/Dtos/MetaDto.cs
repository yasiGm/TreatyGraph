using TreatyGraph.Core.Model;

namespace TreatyGraph.Api.Data;

/// <summary>JSON returned to the browser (property names become camelCase).</summary>
public sealed record MetaDto(IReadOnlyList<CountryMetaDto> Countries, IReadOnlyList<CoverageItem> Coverage);
