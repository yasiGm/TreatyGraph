namespace TreatyGraph.Core.Model;

/// <summary>Static source/coverage metadata, surfaced to the UI via the API's /api/meta endpoint.</summary>
public static class Sources
{
    public static readonly IReadOnlyList<CoverageItem> Coverage = new[]
    {
        new CoverageItem("Gleditsch & Ward state list", 1816, 2017, "Backup list of independent states, used when the COW state list is not available; its codes also translate UCDP data."),
        new CoverageItem("COW Alliances v4.1", 1816, 2012, "Formal alliances (Gibler)."),
        new CoverageItem("ATOP 5.1", 1815, 2018, "Alliance treaty obligations and provisions."),
        new CoverageItem("COW Inter-State Wars v4.0", 1823, 2007, "Inter-state wars only; no records after 2007."),
        new CoverageItem("UCDP/PRIO Armed Conflict v26.1", 1946, 2025, "Inter-state conflicts. 1946-2007: only conflicts COW does not already list for that pair; from 2008 it continues the COW wars."),
        new CoverageItem("UCDP Candidate Events (preliminary)", 2026, 2026, "Conflicts between governments in the current year, from UCDP's monthly preliminary data; may change."),
        new CoverageItem("UCDP External Support 18.1", 1975, 2017, "Confirmed support to a side in a war: helping a country's government, or helping its enemies."),
        new CoverageItem("COW Diplomatic Exchange v2006.1", 1817, 2005, "5-year snapshots, not continuous."),
        new CoverageItem("Wikidata (diplomatic relations)", 1816, 2026, "Diplomatic relations between today's countries: dates established or ended when known, plus a 2026 snapshot."),
        new CoverageItem("Archigos 4.1 (leaders)", 1848, 2015, "All countries covered by Archigos."),
        new CoverageItem("Wikidata (heads of state and government)", 1816, 2026, "Current states only; history is uneven. Archigos fills earlier years."),
    };
}
