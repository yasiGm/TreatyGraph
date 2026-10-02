namespace TreatyGraph.Api.Data;

/// <summary>Row read by Dapper (settable properties + parameterless constructor).</summary>
public sealed class CountryRow
{
    public int Ccode { get; set; }
    public string Abbr { get; set; } = "";
    public string Name { get; set; } = "";
}
