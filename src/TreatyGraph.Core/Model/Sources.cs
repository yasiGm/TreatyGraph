namespace TreatyGraph.Core.Model;

/// <summary>Static source/coverage metadata, surfaced to the UI via the API's /api/meta endpoint.</summary>
public static class Sources
{
    public static readonly IReadOnlyList<CoverageItem> Coverage = new[]
    {
        new CoverageItem("COW Alliances v4.1", 1816, 2012, "Formal alliances (Gibler)."),
        new CoverageItem("ATOP 5.1", 1815, 2018, "Alliance treaty obligations and provisions."),
        new CoverageItem("COW Inter-State Wars v4.0", 1823, 2007, "Inter-state wars only; no records after 2007."),
        new CoverageItem("COW Diplomatic Exchange v2006.1", 1817, 2005, "5-year snapshots, not continuous."),
        new CoverageItem("Archigos 4.1 (leaders)", 1848, 2015, "Only for the pilot countries in this build."),
    };
}
