using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Diplomacy;

/// <summary>
/// Wikidata "diplomatic relation" (P530) statements between today's countries, prepared as
/// ccode1, ccode2, start, end (dates as YYYY, YYYY-MM or YYYY-MM-DD; empty when unknown).
/// Continues the COW diplomatic data, which ends in 2005. Each pair gets an event when relations
/// were established or ended (when Wikidata knows the date), and a snapshot for today when
/// relations are in place but their start date is unknown.
/// </summary>
public sealed class WikidataDiplomacyRule : IEtlRule<DyadEvent>
{
    public const string Source = "Wikidata (diplomatic relations)";
    public const int SnapshotYear = 2026;

    public IReadOnlyList<DyadEvent> Load(SourceFiles files, CountryDirectory countries) =>
        File.Exists(files.WikidataDiplomacy)
            ? FromRows(CsvReader.ReadDictionaries(files.WikidataDiplomacy), countries)
            : Array.Empty<DyadEvent>();

    public List<DyadEvent> FromRows(IEnumerable<Dictionary<string, string>> rows, CountryDirectory countries)
    {
        var pairs = new Dictionary<(int, int), List<(DatePart? Start, DatePart? End)>>();
        foreach (var r in rows)
        {
            var a = Num.ParseInt(r.Get("ccode1"));
            var b = Num.ParseInt(r.Get("ccode2"));
            if (a is null || b is null || a == b || !countries.Contains(a.Value) || !countries.Contains(b.Value))
            {
                continue;
            }

            var key = (Math.Min(a.Value, b.Value), Math.Max(a.Value, b.Value));
            if (!pairs.TryGetValue(key, out var list))
            {
                list = new List<(DatePart?, DatePart?)>();
                pairs[key] = list;
            }

            list.Add((ParseDate(r.Get("start")), ParseDate(r.Get("end"))));
        }

        var events = new List<DyadEvent>();
        foreach (var ((lo, hi), list) in pairs)
        {
            var current = list.Any(x => x.End is null);
            var starts = list.Where(x => x.Start is not null).Select(x => x.Start!.Value).Distinct().OrderBy(d => d.Key());
            var ends = list.Where(x => x.End is not null).Select(x => x.End!.Value).Distinct().OrderBy(d => d.Key());

            foreach (var s in starts)
            {
                events.Add(DyadEvent.Create(lo, hi, EventTypes.Diplomacy, Source, s, null,
                    "Diplomatic relations established",
                    $"{countries.Name(lo)} and {countries.Name(hi)} established diplomatic relations (Wikidata)."));
            }

            foreach (var e in ends)
            {
                events.Add(DyadEvent.Create(lo, hi, EventTypes.Diplomacy, Source, e, null,
                    "Diplomatic relations ended",
                    $"{countries.Name(lo)} and {countries.Name(hi)} ended or broke off diplomatic relations (Wikidata)."));
            }

            if (current)
            {
                events.Add(DyadEvent.Create(lo, hi, EventTypes.Diplomacy, Source, new DatePart(SnapshotYear, null, null), null,
                    "Diplomatic relations in place",
                    $"Wikidata lists diplomatic relations between {countries.Name(lo)} and {countries.Name(hi)} as of {SnapshotYear}."));
            }
        }

        return events;
    }

    // "1972", "1972-02" or "1972-02-21"; anything else (empty, "unknown value") -> null.
    private static DatePart? ParseDate(string text)
    {
        var parts = text.Trim().Split('-');
        if (parts.Length == 0 || Num.ParseInt(parts[0]) is not int year || year < 1000)
        {
            return null;
        }

        int? month = parts.Length > 1 ? Num.ParseInt(parts[1]) : null;
        int? day = parts.Length > 2 ? Num.ParseInt(parts[2]) : null;
        return new DatePart(year, month is >= 1 and <= 12 ? month : null, month is >= 1 and <= 12 && day is >= 1 and <= 31 ? day : null);
    }
}
