using TreatyGraph.Core.Etl.Diplomacy;
using TreatyGraph.Core.Model;
using static TreatyGraph.Tests.TestHelpers;

namespace TreatyGraph.Tests;

public class DiplomacyRuleTests
{
    [Fact]
    public void Only_changes_in_the_level_of_representation_become_events()
    {
        var rows = new Dictionary<(int, int), List<(int Year, int L1, int L2)>>
        {
            [(365, 630)] = new()
            {
                (1859, 2, 2),
                (1864, 2, 2), // unchanged -> no event
                (1869, 3, 2),
                (1874, 0, 0),
            },
        };

        var events = new DiplomacyRule().FromRows(rows, Countries());

        Assert.Equal(3, events.Count);
        Assert.Equal("1859", events[0].Start.ToString());
        Assert.Equal("Diplomatic representation: Russia: minister · Iran: minister", events[0].Title);
        Assert.Equal("Diplomatic representation: Russia: ambassador · Iran: minister", events[1].Title);
        Assert.Contains("after the 1864 snapshot", events[1].Detail);
        Assert.Equal("No diplomatic representation recorded", events[2].Title);
    }

    [Fact]
    public void A_pair_that_starts_with_no_representation_gets_no_initial_event()
    {
        var rows = new Dictionary<(int, int), List<(int Year, int L1, int L2)>>
        {
            [(200, 220)] = new() { (1920, 0, 0), (1925, 0, 0), (1930, 0, 2) },
        };

        var e = Assert.Single(new DiplomacyRule().FromRows(rows, Countries()));

        Assert.Equal("1930", e.Start.ToString());
    }

    [Fact]
    public void Snapshots_from_1950_to_1965_carry_a_warning()
    {
        var rows = new Dictionary<(int, int), List<(int Year, int L1, int L2)>>
        {
            [(365, 630)] = new() { (1940, 3, 3), (1950, 9, 9) },
        };

        var events = new DiplomacyRule().FromRows(rows, Countries());

        Assert.Contains("unavailable", events.Last().Detail);
    }
}
