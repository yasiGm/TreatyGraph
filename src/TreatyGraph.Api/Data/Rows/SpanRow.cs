namespace TreatyGraph.Api.Data;

/// <summary>Row read by Dapper (settable properties + parameterless constructor).</summary>
public sealed class SpanRow
{
    public int Ccode { get; set; }
    public int StartYear { get; set; }
    public int EndYear { get; set; }
}
