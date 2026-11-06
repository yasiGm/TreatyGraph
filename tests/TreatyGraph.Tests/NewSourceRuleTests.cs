using TreatyGraph.Core.Etl.Diplomacy;
using TreatyGraph.Core.Etl.Wars;
using TreatyGraph.Core.Model;
using static TreatyGraph.Tests.TestHelpers;

namespace TreatyGraph.Tests;

public class NewSourceRuleTests
{
    [Fact]
    public void Wikidata_diplomacy_gives_established_ended_and_current_events()
    {
        var rows = new[]
        {
            Row(("ccode1", "630"), ("ccode2", "200"), ("start", "1942"), ("end", "1980-04")),
            Row(("ccode1", "200"), ("ccode2", "630"), ("start", ""), ("end", "")), // other direction, still in place
            Row(("ccode1", "220"), ("ccode2", "999"), ("start", "1950"), ("end", "")), // unknown country -> dropped
        };

        var events = new WikidataDiplomacyRule().FromRows(rows, Countries());

        Assert.Equal(3, events.Count);
        Assert.All(events, e => Assert.Equal((200, 630), (e.CcodeLo, e.CcodeHi)));
        Assert.Contains(events, e => e.Title == "Diplomatic relations established" && e.Start.ToString() == "1942");
        Assert.Contains(events, e => e.Title == "Diplomatic relations ended" && e.Start.ToString() == "1980-04");
        Assert.Contains(events, e => e.Title == "Diplomatic relations in place" && e.Start.Year == WikidataDiplomacyRule.SnapshotYear);
    }

    [Fact]
    public void Ucdp_before_2008_skips_pairs_that_cow_already_lists()
    {
        Dictionary<string, string> Year(string id, string a, string b, string year) =>
            Row(("conflict_id", id), ("location", "Test"), ("type_of_conflict", "2"), ("year", year),
                ("gwno_a", a), ("gwno_b", b), ("intensity_level", "1"));

        var rows = new[]
        {
            Year("1", "200", "630", "1950"), // COW has a war for this pair in 1950 -> skipped
            Year("2", "220", "630", "1960"), // COW has nothing -> kept
        };
        var cow = new HashSet<(int, int, int)> { (200, 630, 1950) };

        var events = new UcdpInterStateRule().FromRows(rows, Countries(), UcdpInterStateRule.EarliestYear, cow);

        var e = Assert.Single(events);
        Assert.Equal((220, 630), (e.CcodeLo, e.CcodeHi));
        Assert.Contains("Not in the COW war list", e.Detail);
    }
}

public class GleditschWardTests
{
    [Fact]
    public void Gleditsch_ward_list_is_read_and_codes_become_cow_codes()
    {
        var lines = new[]
        {
            "2\tUSA\tUnited States of America\t01:01:1816\t31:12:2017",
            "260\tGFR\tGerman Federal Republic\t21:09:1949\t31:12:2017",
            "340\tSER\tSerbia\t05:06:2006\t31:12:2017",
            "971\tNAU\tNauru\t31:01:1968\t31:12:2017",
            "not a data line",
        };

        var countries = TreatyGraph.Core.Etl.Countries.GleditschWard.FromLines(lines, 2026);

        Assert.True(countries.Contains(2));
        Assert.True(countries.Contains(260)); // West Germany until 1990
        Assert.True(countries.Contains(255)); // unified Germany from 1990, COW code
        Assert.True(countries.Contains(345)); // Serbia, COW code
        Assert.True(countries.Contains(970)); // Nauru, COW code
        Assert.False(countries.Contains(340));
        Assert.Equal(4 + 1, countries.All.Count);
    }

    [Theory]
    [InlineData(260, 1985, 260)]
    [InlineData(260, 1995, 255)]
    [InlineData(678, 1995, 679)]
    [InlineData(2, 2000, 2)]
    public void Gw_codes_translate_by_year(int gw, int year, int cow) =>
        Assert.Equal(cow, TreatyGraph.Core.Etl.Countries.GleditschWard.ToCow(gw, year));
}

public class ExtraTerritoryTests
{
    [Fact]
    public void Palestine_vatican_and_territories_are_added_with_codes_above_cow()
    {
        var countries = TreatyGraph.Core.Etl.Countries.CountryLoader.WithExtraTerritories(Countries(), 2026);

        Assert.True(countries.Contains(200)); // the normal states stay
        Assert.Equal("Palestine", countries.Name(1001));
        Assert.Equal("Vatican City", countries.Name(1002));
        Assert.All(ExtraTerritories.All(2026), t => Assert.True(t.Ccode >= ExtraTerritories.FirstCode));
    }
}

public class UcdpCandidateTests
{
    [Fact]
    public void Candidate_events_between_governments_are_grouped_by_pair()
    {
        Dictionary<string, string> Ev(string start, string end, string a, string gwa, string b, string gwb, string best, string type = "1") =>
            Row(("type_of_violence", type), ("conflict_new_id", "9"), ("date_start", start), ("date_end", end),
                ("side_a", a), ("gwnoa", gwa), ("side_b", b), ("gwnob", gwb), ("best", best));

        var rows = new[]
        {
            Ev("2026-02-28", "2026-02-28", "Government of Iran", "630", "Government of United Kingdom", "", "40"),
            Ev("2026-03-02", "2026-03-03", "Government of Iran", "630", "Government of United Kingdom", "", "10"),
            Ev("2025-12-30", "2025-12-30", "Government of Iran", "630", "Government of France", "220", "5"),     // 2025: left to the yearly file
            Ev("2026-04-01", "2026-04-01", "Government of Iran", "630", "Some rebel group", "", "3"),             // not a government
            Ev("2026-04-01", "2026-04-01", "Government of Iran", "630", "Government of France", "220", "3", "3"), // one-sided violence
        };

        var events = new UcdpCandidateRule().FromRows(rows, Countries());

        var e = Assert.Single(events);
        Assert.Equal((200, 630), (e.CcodeLo, e.CcodeHi));
        Assert.Equal(EventTypes.War, e.Type);
        Assert.Equal("2026-02-28", e.Start.ToString());
        Assert.Equal("2026-03-03", e.End!.Value.ToString());
        Assert.Contains("2 recorded event(s)", e.Detail);
        Assert.Contains("50 deaths", e.Detail);
    }
}

public class ExternalSupportTests
{
    [Fact]
    public void Support_for_a_government_and_against_one_become_two_kinds_of_events()
    {
        Dictionary<string, string> T(string year, string sup, string rec, string recName, string opp, string oppName, string kinds, string alleged = "0") =>
            Row(("year", year), ("supporter", sup), ("recipient", rec), ("recipient_name", recName), ("opponent", opp),
                ("opponent_name", oppName), ("conflict", "1"), ("location", "Iran"), ("alleged", alleged), ("elements", "0"), ("kinds", kinds));

        var rows = new[]
        {
            // Russia backs a rebel group against Iran's government in 1980 and 1981 -> one proxy period
            T("1980", "365", "", "Rebels", "630", "Government of Iran", "weapons"),
            T("1981", "365", "", "Rebels", "630", "Government of Iran", "funding"),
            // France helps Iran's government against the same rebels -> support
            T("1990", "220", "630", "Government of Iran", "", "Rebels", "training"),
            // alleged only -> ignored
            T("1995", "200", "630", "Government of Iran", "", "Rebels", "troops", alleged: "1"),
        };

        var events = new TreatyGraph.Core.Etl.Support.ExternalSupportRule().FromRows(rows, Countries());

        Assert.Equal(2, events.Count);
        var proxy = Assert.Single(events, e => e.Type == EventTypes.Proxy);
        Assert.Equal((365, 630), (proxy.CcodeLo, proxy.CcodeHi));
        Assert.Equal("1980", proxy.Start.ToString());
        Assert.Equal("1981", proxy.End!.Value.ToString());
        Assert.Contains("funding, weapons", proxy.Detail);
        var support = Assert.Single(events, e => e.Type == EventTypes.Support);
        Assert.Equal((220, 630), (support.CcodeLo, support.CcodeHi));
    }
}
