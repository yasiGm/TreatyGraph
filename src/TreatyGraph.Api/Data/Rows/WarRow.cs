namespace TreatyGraph.Api.Data;

/// <summary>Row read by Dapper (settable properties + parameterless constructor).</summary>
public sealed class WarRow
{
    public string WarName { get; set; } = "";
    public int StartYear { get; set; }
    public int? StartMonth { get; set; }
    public int? StartDay { get; set; }
    public int EndYear { get; set; }
    public int? EndMonth { get; set; }
    public int? EndDay { get; set; }
    public int SortKey { get; set; }
    public string? Result { get; set; }
    public bool IsInitiator { get; set; }
    public string Opponents { get; set; } = "";
    public string Allies { get; set; } = "";
    public int? BattleDeaths { get; set; }
}
