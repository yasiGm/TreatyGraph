using System.Text;

namespace TreatyGraph.Core.Csv;

/// <summary>
/// Minimal RFC-4180-style CSV reader. Handles quoted fields, escaped quotes ("")
/// and CRLF / LF / lone-CR line endings (some COW files use CR only).
/// </summary>
public static class CsvReader
{
    public static IReadOnlyList<Dictionary<string, string>> ReadDictionaries(string path, Encoding? encoding = null)
    {
        var text = File.ReadAllText(path, encoding ?? Encoding.UTF8);
        return ParseDictionaries(text);
    }

    public static IReadOnlyList<Dictionary<string, string>> ParseDictionaries(string text)
    {
        var rows = ParseRows(text);
        var result = new List<Dictionary<string, string>>();
        if (rows.Count == 0)
        {
            return result;
        }

        var header = rows[0].Select(h => h.Trim()).ToArray();
        for (var i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Count == 1 && row[0].Length == 0)
            {
                continue; // blank line
            }

            var dict = new Dictionary<string, string>(header.Length, StringComparer.Ordinal);
            for (var c = 0; c < header.Length; c++)
            {
                dict[header[c]] = c < row.Count ? row[c] : string.Empty;
            }

            result.Add(dict);
        }

        return result;
    }

    public static List<List<string>> ParseRows(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(ch);
                }

                continue;
            }

            switch (ch)
            {
                case '"':
                    inQuotes = true;
                    break;
                case ',':
                    row.Add(field.ToString());
                    field.Clear();
                    break;
                case '\r':
                    if (i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        i++;
                    }

                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = new List<string>();
                    break;
                case '\n':
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = new List<string>();
                    break;
                default:
                    field.Append(ch);
                    break;
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        return rows;
    }
}
