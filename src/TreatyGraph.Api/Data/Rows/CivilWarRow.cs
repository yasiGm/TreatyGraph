namespace TreatyGraph.Api.Data;

/// <summary>Row read by Dapper (settable properties + parameterless constructor).</summary>
public sealed class CivilWarRow
{
    public string WarName { get; set; } = "";
    public int StartYear { get; set; }
    public int? StartMonth { get; set; }
    public int? StartDay { get; set; }
    public int? EndYear { get; set; }
    public int? EndMonth { get; set; }
    public int? EndDay { get; set; }
    public int SortKey { get; set; }
    public string Sides { get; set; } = "";
    public bool IsInternationalized { get; set; }
}
