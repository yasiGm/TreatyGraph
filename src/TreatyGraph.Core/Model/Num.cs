using System.Globalization;

namespace TreatyGraph.Core.Model;

public static class Num
{
    /// <summary>Parses "1822", "1822.0", " 4 " ... ; returns null for blank / unparsable input.</summary>
    public static int? ParseInt(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return null;
        }

        return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
            ? (int?)d
            : null;
    }
}
