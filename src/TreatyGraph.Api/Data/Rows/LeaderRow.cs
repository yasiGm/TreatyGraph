namespace TreatyGraph.Api.Data;

/// <summary>Row read by Dapper (settable properties + parameterless constructor).</summary>
public sealed class LeaderRow
{
    public string Name { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ExitType { get; set; } = "";
}
