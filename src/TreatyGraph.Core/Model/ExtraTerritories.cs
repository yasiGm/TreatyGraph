namespace TreatyGraph.Core.Model;

/// <summary>
/// Places that appear in the newer sources (Wikidata) but are not states in the COW or Gleditsch-Ward lists:
/// Palestine, Vatican City and self-governing territories. They get our own codes from 1001 up, so they can
/// never clash with a COW code (COW codes stop below 1000). Spans start roughly when the place got its
/// current status; they are only used to show the place in the country lists.
/// </summary>
public static class ExtraTerritories
{
    public const int FirstCode = 1001;

    public static IReadOnlyList<Country> All(int currentYear) => new[]
    {
        Make(1001, "PSE", "Palestine", 1988, currentYear),            // State of Palestine declared 1988
        Make(1002, "VAT", "Vatican City", 1929, currentYear),         // Lateran Treaty 1929
        Make(1003, "HKG", "Hong Kong", 1842, currentYear),            // British colony 1842, Chinese SAR since 1997
        Make(1004, "MAC", "Macau", 1816, currentYear),                // Portuguese until 1999, Chinese SAR since
        Make(1005, "GRL", "Greenland", 1979, currentYear),            // home rule within Denmark 1979
        Make(1006, "ABW", "Aruba", 1986, currentYear),                // separate country in the Kingdom of the Netherlands 1986
        Make(1007, "BMU", "Bermuda", 1816, currentYear),              // British overseas territory
        Make(1008, "CYM", "Cayman Islands", 1962, currentYear),       // separate British territory 1962
        Make(1009, "COK", "Cook Islands", 1965, currentYear),         // self-governing with New Zealand 1965
        Make(1010, "PYF", "French Polynesia", 1946, currentYear),     // French overseas territory 1946
        Make(1011, "IMN", "Isle of Man", 1816, currentYear),          // British Crown dependency
        Make(1012, "ANT", "Netherlands Antilles", 1954, 2010),        // dissolved 2010
        Make(1013, "NCL", "New Caledonia", 1946, currentYear),        // French overseas territory 1946
        Make(1014, "NIU", "Niue", 1974, currentYear),                 // self-governing with New Zealand 1974
    };

    /// <summary>Wikidata's English names for these places, so the prepared data files can use their codes.</summary>
    public static readonly IReadOnlyDictionary<string, int> ByWikidataName = new Dictionary<string, int>
    {
        ["Palestine"] = 1001, ["State of Palestine"] = 1001, ["Vatican City"] = 1002, ["Hong Kong"] = 1003,
        ["Macau"] = 1004, ["Greenland"] = 1005, ["Aruba"] = 1006, ["Bermuda"] = 1007, ["Cayman Islands"] = 1008,
        ["Cook Islands"] = 1009, ["French Polynesia"] = 1010, ["Isle of Man"] = 1011, ["Netherlands Antilles"] = 1012,
        ["New Caledonia"] = 1013, ["Niue"] = 1014,
    };

    private static Country Make(int code, string abbr, string name, int start, int end) =>
        new(code, abbr, name, new[] { new YearSpan(start, end) });
}
