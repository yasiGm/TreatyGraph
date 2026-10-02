namespace TreatyGraph.Core.Model;

/// <summary>One thing that happened between a pair of countries. CcodeLo &lt; CcodeHi always.</summary>
public sealed record DyadEvent(
    int CcodeLo,
    int CcodeHi,
    string Type,
    string Source,
    DatePart Start,
    DatePart? End,
    bool Ongoing,
    bool LeftCensored,
    string Title,
    string Detail)
{
    public int SortKey => Start.Key();

    public static DyadEvent Create(
        int a,
        int b,
        string type,
        string source,
        DatePart start,
        DatePart? end,
        string title,
        string detail,
        bool ongoing = false,
        bool leftCensored = false) =>
        new(Math.Min(a, b), Math.Max(a, b), type, source, start, end, ongoing, leftCensored, title, detail);
}
