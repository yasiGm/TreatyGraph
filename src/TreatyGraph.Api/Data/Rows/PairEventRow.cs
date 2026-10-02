namespace TreatyGraph.Api.Data;

/// <summary>Row read by Dapper (settable properties + parameterless constructor).</summary>
public sealed class PairEventRow
{
    public string EventType { get; set; } = "";
    public string Source { get; set; } = "";
    public int StartYear { get; set; }
    public int? StartMonth { get; set; }
    public int? StartDay { get; set; }
    public int? EndYear { get; set; }
    public int? EndMonth { get; set; }
    public int? EndDay { get; set; }
    public bool IsOngoing { get; set; }
    public bool IsLeftCensored { get; set; }
    public int SortKey { get; set; }
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
}
