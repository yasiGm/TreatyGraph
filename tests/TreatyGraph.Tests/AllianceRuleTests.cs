using TreatyGraph.Core.Etl.Alliances;
using TreatyGraph.Core.Model;
using static TreatyGraph.Tests.TestHelpers;

namespace TreatyGraph.Tests;

public class AllianceRuleTests
{
    [Fact]
    public void Cow_defense_pact_becomes_one_event_with_canonical_pair_order()
    {
        var rows = new[]
        {
            Row(("version4id", "105"), ("ccode1", "630"), ("ccode2", "365"),
                ("dyad_st_year", "1921"), ("dyad_st_month", "2"), ("dyad_st_day", "26"),
                ("dyad_end_year", "1941"), ("dyad_end_month", "8"), ("dyad_end_day", "25"),
                ("left_censor", "0"), ("right_censor", "0"),
                ("defense", "1"), ("neutrality", "0"), ("nonaggression", "0"), ("entente", "0"), ("asymmetric", "0")),
        };

        var e = Assert.Single(new CowAllianceRule().FromRows(rows));

        Assert.Equal(365, e.CcodeLo);
        Assert.Equal(630, e.CcodeHi);
        Assert.Equal("Defense pact", e.Title);
        Assert.Equal("1921-02-26", e.Start.ToString());
        Assert.Equal("1941-08-25", e.End!.Value.ToString());
        Assert.False(e.Ongoing);
    }

    [Fact]
    public void Cow_open_ended_alliance_is_ongoing_only_when_right_censored()
    {
        Dictionary<string, string> Make(string censor) =>
            Row(("version4id", "1"), ("ccode1", "200"), ("ccode2", "235"),
                ("dyad_st_year", "1816"), ("dyad_st_month", "1"), ("dyad_st_day", "1"),
                ("dyad_end_year", ""), ("dyad_end_month", ""), ("dyad_end_day", ""),
                ("left_censor", "1"), ("right_censor", censor),
                ("defense", "1"), ("neutrality", "0"), ("nonaggression", "1"), ("entente", "0"), ("asymmetric", "0"));

        var censored = Assert.Single(new CowAllianceRule().FromRows(new[] { Make("1") }));
        var notCensored = Assert.Single(new CowAllianceRule().FromRows(new[] { Make("0") }));

        Assert.True(censored.Ongoing);
        Assert.True(censored.LeftCensored);
        Assert.False(notCensored.Ongoing);
        Assert.Null(censored.End);
    }

    private static Dictionary<string, string> Member(string atopId, int country, string? enter, string? exit)
    {
        var (ey, em, ed) = Split(enter);
        var (xy, xm, xd) = Split(exit);
        return Row(("atopid", atopId), ("member", country.ToString()),
            ("yrent", ey), ("moent", em), ("dayent", ed),
            ("yrexit", xy), ("moexit", xm), ("dayexit", xd));
    }

    private static (string, string, string) Split(string? date)
    {
        if (date is null)
        {
            return ("0", "0", "0");
        }

        var p = date.Split('-');
        return (p[0], p[1], p[2]);
    }

    private static Dictionary<string, string> Alliance(string atopId) =>
        Row(("atopid", atopId), ("defense", "1"), ("offense", "0"), ("neutral", "0"), ("nonagg", "0"), ("consul", "1"));

    [Fact]
    public void Atop_pair_window_runs_from_the_later_entry_to_the_earlier_exit()
    {
        var members = new[]
        {
            Member("9", 200, "1900-01-01", null),
            Member("9", 220, "1905-03-04", "1910-06-01"),
            Member("9", 255, "1902-05-06", "1908-07-08"),
        };

        var events = new AtopAllianceRule().FromRows(members, new[] { Alliance("9") });

        Assert.Equal(3, events.Count);
        var frGe = events.Single(e => e.CcodeLo == 220 && e.CcodeHi == 255);
        Assert.Equal("1905-03-04", frGe.Start.ToString());
        Assert.Equal("1908-07-08", frGe.End!.Value.ToString());
        var ukFr = events.Single(e => e.CcodeLo == 200 && e.CcodeHi == 220);
        Assert.Equal("1905-03-04", ukFr.Start.ToString());
        Assert.Equal("1910-06-01", ukFr.End!.Value.ToString());
        Assert.All(events, e => Assert.Equal("Defense pact", e.Title));
    }

    [Fact]
    public void Atop_consecutive_phases_of_one_alliance_are_merged()
    {
        var members = new[]
        {
            Member("7", 200, "1900-01-01", "1910-06-01"),
            Member("7", 200, "1910-06-01", null),
            Member("7", 220, "1900-01-01", null),
        };

        var e = Assert.Single(new AtopAllianceRule().FromRows(members, new[] { Alliance("7") }));

        Assert.Equal("1900-01-01", e.Start.ToString());
        Assert.Null(e.End);
        Assert.True(e.Ongoing);
    }
}
