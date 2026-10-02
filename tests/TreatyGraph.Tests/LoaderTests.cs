using TreatyGraph.Core.Etl.Countries;
using TreatyGraph.Core.Etl.Leaders;
using static TreatyGraph.Tests.TestHelpers;

namespace TreatyGraph.Tests;

public class LoaderTests
{
    [Fact]
    public void Country_with_two_membership_spans_keeps_both()
    {
        var rows = new[]
        {
            Row(("stateabb", "CUB"), ("ccode", "40"), ("statenme", "Cuba"), ("styear", "1902"), ("endyear", "1906")),
            Row(("stateabb", "CUB"), ("ccode", "40"), ("statenme", "Cuba"), ("styear", "1909"), ("endyear", "2024")),
        };

        var directory = CountryLoader.FromRows(rows);

        var cuba = Assert.Single(directory.All);
        Assert.Equal(2, cuba.Spans.Count);
        Assert.Equal("Cuba", directory.Name(40));
        Assert.Equal("Unknown (COW 8152)", directory.Name(8152));
    }

    [Fact]
    public void Leaders_are_parsed_with_iso_dates()
    {
        var rows = new[]
        {
            Row(("ccode", "630"), ("leader", "Khatami"), ("start", "1997-08-03"), ("end", "2005-08-03"), ("exit", "Regular")),
        };

        var leader = Assert.Single(new LeaderRule().FromRows(rows));

        Assert.Equal(new DateOnly(1997, 8, 3), leader.Start);
        Assert.Equal("Regular", leader.ExitType);
    }
}
