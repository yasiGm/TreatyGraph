using TreatyGraph.Core.Model;

namespace TreatyGraph.Tests;

public class DatePartTests
{
    [Theory]
    [InlineData("1921", "2", "26", "1921-02-26")]
    [InlineData("1921", "2", "-9", "1921-02")]
    [InlineData("1921", "-9", "-9", "1921")]
    [InlineData("1921", "", "", "1921")]
    [InlineData("1921.0", "13", "5", "1921")]
    public void Clean_keeps_only_what_is_known(string year, string month, string day, string expected)
    {
        var date = DatePart.Clean(year, month, day);

        Assert.NotNull(date);
        Assert.Equal(expected, date!.Value.ToString());
    }

    [Theory]
    [InlineData("-8")]
    [InlineData("-9")]
    [InlineData("")]
    public void Clean_returns_null_when_the_year_is_missing(string year)
    {
        Assert.Null(DatePart.Clean(year, "1", "1"));
    }

    [Fact]
    public void Day_is_ignored_when_the_month_is_unknown()
    {
        var date = DatePart.Clean("1900", "-9", "15");

        Assert.Equal(new DatePart(1900, null, null), date);
    }

    [Fact]
    public void Key_defaults_to_earliest_for_starts_and_latest_for_ends()
    {
        var d = new DatePart(1900, null, null);

        Assert.Equal(19000101, d.Key());
        Assert.Equal(19001228, d.Key(end: true));
    }
}
