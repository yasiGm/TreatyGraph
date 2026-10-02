using TreatyGraph.Core.Model;

namespace TreatyGraph.Tests;

internal static class TestHelpers
{
    public static Dictionary<string, string> Row(params (string Key, string Value)[] cells) =>
        cells.ToDictionary(c => c.Key, c => c.Value);

    public static CountryDirectory Countries() => new(new[]
    {
        new Country(200, "UKG", "United Kingdom", new[] { new YearSpan(1816, 2024) }),
        new Country(220, "FRN", "France", new[] { new YearSpan(1816, 2024) }),
        new Country(255, "GMY", "Germany", new[] { new YearSpan(1816, 2024) }),
        new Country(365, "RUS", "Russia", new[] { new YearSpan(1816, 2024) }),
        new Country(630, "IRN", "Iran", new[] { new YearSpan(1855, 2024) }),
    });
}
