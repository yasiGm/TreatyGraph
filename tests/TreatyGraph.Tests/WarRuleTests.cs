using TreatyGraph.Core.Etl.Wars;
using TreatyGraph.Core.Model;
using static TreatyGraph.Tests.TestHelpers;

namespace TreatyGraph.Tests;

public class WarRuleTests
{
    private static Dictionary<string, string> Participant(
        string warNum, string warName, int ccode, string side, string startYear, string endYear,
        string outcome = "1", string initiator = "2", string deaths = "1000") =>
        Row(("WarNum", warNum), ("WarName", warName), ("ccode", ccode.ToString()), ("Side", side),
            ("StartYear1", startYear), ("StartMonth1", "4"), ("StartDay1", "7"),
            ("EndYear1", endYear), ("EndMonth1", "9"), ("EndDay1", "30"),
            ("StartYear2", "-8"), ("StartMonth2", "-8"), ("StartDay2", "-8"),
            ("EndYear2", "-8"), ("EndMonth2", "-8"), ("EndDay2", "-8"),
            ("Outcome", outcome), ("Initiator", initiator), ("BatDeath", deaths));

    [Fact]
    public void Opposing_sides_produce_a_war_event_and_same_side_a_coalition_event()
    {
        var rows = new[]
        {
            Participant("1", "Crimean", 200, "1", "1854", "1856", outcome: "1"),
            Participant("1", "Crimean", 220, "1", "1854", "1856", outcome: "1"),
            Participant("1", "Crimean", 365, "2", "1853", "1856", outcome: "2", initiator: "1", deaths: "100000"),
        };

        var pairEvents = new InterStateWarRule().FromRows(rows, Countries());

        Assert.Equal(3, pairEvents.Count);
        var ukRu = pairEvents.Single(e => e.CcodeLo == 200 && e.CcodeHi == 365);
        Assert.Equal(EventTypes.War, ukRu.Type);
        Assert.Equal("Crimean War — on opposing sides", ukRu.Title);
        Assert.Equal("1854-04-07", ukRu.Start.ToString()); // later of the two starts
        var ukFr = pairEvents.Single(e => e.CcodeLo == 200 && e.CcodeHi == 220);
        Assert.Equal(EventTypes.Coalition, ukFr.Type);
    }

    [Fact]
    public void War_name_that_already_contains_war_is_not_suffixed()
    {
        var rows = new[]
        {
            Participant("2", "World War I", 200, "1", "1914", "1918"),
            Participant("2", "World War I", 255, "2", "1914", "1918", outcome: "2"),
        };

        var e = Assert.Single(new InterStateWarRule().FromRows(rows, Countries()));

        Assert.Equal("World War I — on opposing sides", e.Title);
    }

    [Fact]
    public void Country_profile_rows_list_opponents_allies_and_result()
    {
        var rows = new[]
        {
            Participant("1", "Crimean", 200, "1", "1854", "1856"),
            Participant("1", "Crimean", 220, "1", "1854", "1856"),
            Participant("1", "Crimean", 365, "2", "1853", "1856", outcome: "2", initiator: "1"),
        };

        var profile = new CountryWarRule().FromRows(rows, Countries());

        var uk = profile.Single(p => p.Ccode == 200);
        Assert.Equal("Crimean War", uk.Name);
        Assert.Equal("winner", uk.Result);
        Assert.Equal(new[] { "Russia" }, uk.Opponents);
        Assert.Equal(new[] { "France" }, uk.Allies);
        var ru = profile.Single(p => p.Ccode == 365);
        Assert.True(ru.Initiator);
        Assert.Equal("loser", ru.Result);
    }

    [Fact]
    public void Civil_war_sides_with_missing_names_are_labelled()
    {
        var rows = new[]
        {
            Row(("WarName", "Dhofar Rebellion"), ("CcodeA", "630"), ("SideA", "Iran"), ("CcodeB", "-8"), ("SideB", "-8"),
                ("Intnl", "1"), ("StartYear1", "1973"), ("StartMonth1", "10"), ("StartDay1", "-9"),
                ("EndYear1", "1975"), ("EndMonth1", "12"), ("EndDay1", "11")),
        };

        var civil = Assert.Single(new CivilWarRule().FromRows(rows, Countries()));

        Assert.Equal(630, civil.Ccode);
        Assert.Equal("Iran vs other parties", civil.Sides);
        Assert.True(civil.Internationalized);
        Assert.Equal("1973-10", civil.Start.ToString());
    }
}
