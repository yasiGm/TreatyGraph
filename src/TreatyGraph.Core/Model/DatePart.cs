namespace TreatyGraph.Core.Model;

/// <summary>
/// A possibly partial date. COW data often know only the year (or year and month).
/// </summary>
public readonly record struct DatePart(int Year, int? Month, int? Day)
{
    /// <summary>
    /// Sortable yyyymmdd key. For start dates unknown parts default to the earliest value,
    /// for end dates (<paramref name="end"/> = true) to the latest (Dec / day 28).
    /// </summary>
    public int Key(bool end = false)
    {
        var month = Month ?? (end ? 12 : 1);
        var day = Day ?? (end ? 28 : 1);
        return Year * 10000 + month * 100 + day;
    }

    /// <summary>"YYYY", "YYYY-MM" or "YYYY-MM-DD" depending on how much is known.</summary>
    public override string ToString()
    {
        if (Month is null)
        {
            return Year.ToString("D4");
        }

        if (Day is null)
        {
            return $"{Year:D4}-{Month.Value:D2}";
        }

        return $"{Year:D4}-{Month.Value:D2}-{Day.Value:D2}";
    }

    /// <summary>
    /// Builds a date from raw COW strings. COW codes "not applicable" as -8 and "unknown" as -9
    /// (or leaves the cell blank); those, and out-of-range months/days, become null parts.
    /// Returns null when the year itself is unusable.
    /// </summary>
    public static DatePart? Clean(string? year, string? month, string? day) =>
        FromInts(Num.ParseInt(year), Num.ParseInt(month), Num.ParseInt(day));

    public static DatePart? FromInts(int? year, int? month, int? day)
    {
        if (year is null || year < 0)
        {
            return null;
        }

        int? m = (month is null || month <= 0 || month > 12) ? null : month;
        int? d = (m is null || day is null || day <= 0 || day > 31) ? null : day;
        return new DatePart(year.Value, m, d);
    }
}
