namespace TreatyGraph.Core.Csv;

public static class RowExtensions
{
    /// <summary>The cell value, or an empty string when the column is missing.</summary>
    public static string Get(this Dictionary<string, string> row, string key) =>
        row.TryGetValue(key, out var v) ? v : string.Empty;
}
