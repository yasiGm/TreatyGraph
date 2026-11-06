using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Support;

/// <summary>
/// UCDP External Support Dataset 18.1 (1975-2017), triad-year version, prepared as esd_triads.csv
/// (year, supporter, recipient, recipient_name, opponent, opponent_name, conflict, location, alleged, elements, kinds;
/// supporter/recipient/opponent are COW codes, empty when that side is not a government).
/// Only confirmed support is used (alleged = 0). Two kinds of pair events come out of it:
/// "support"  - a state helped another country's government in a war;
/// "proxy"    - a state helped someone fighting against another country's government.
/// Consecutive years of the same support are joined into one period.
/// </summary>
public sealed class ExternalSupportRule : IEtlRule<DyadEvent>
{
    public const string Source = "UCDP External Support 18.1";

    public IReadOnlyList<DyadEvent> Load(SourceFiles files, CountryDirectory countries) =>
        File.Exists(files.ExternalSupport)
            ? FromRows(CsvReader.ReadDictionaries(files.ExternalSupport), countries)
            : Array.Empty<DyadEvent>();

    private sealed record Key(string Type, int Supporter, int Other, string Conflict, string Partner);

    public List<DyadEvent> FromRows(IEnumerable<Dictionary<string, string>> rows, CountryDirectory countries)
    {
        var years = new Dictionary<Key, SortedDictionary<int, (HashSet<string> Kinds, bool Elements, string Location)>>();

        void Add(Key key, int year, IEnumerable<string> kinds, bool elements, string location)
        {
            if (!years.TryGetValue(key, out var byYear))
            {
                byYear = new SortedDictionary<int, (HashSet<string>, bool, string)>();
                years[key] = byYear;
            }

            if (!byYear.TryGetValue(year, out var y))
            {
                y = (new HashSet<string>(), elements, location);
                byYear[year] = y;
            }

            y.Kinds.UnionWith(kinds);
        }

        foreach (var r in rows)
        {
            if (r.Get("alleged").Trim() == "1"
                || Num.ParseInt(r.Get("year")) is not int year
                || Num.ParseInt(r.Get("supporter")) is not int supporter
                || !countries.Contains(supporter))
            {
                continue;
            }

            var kinds = r.Get("kinds").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var elements = r.Get("elements").Trim() == "1";
            var conflict = r.Get("conflict").Trim();
            var location = r.Get("location").Trim();

            // helped a government: supporter and that government were on the same side
            if (Num.ParseInt(r.Get("recipient")) is int recipient && recipient != supporter && countries.Contains(recipient))
            {
                Add(new Key(EventTypes.Support, supporter, recipient, conflict, r.Get("opponent_name").Trim()), year, kinds, elements, location);
            }

            // helped someone fighting a government: supporter worked against that government
            if (Num.ParseInt(r.Get("opponent")) is int opponent && opponent != supporter && countries.Contains(opponent))
            {
                Add(new Key(EventTypes.Proxy, supporter, opponent, conflict, r.Get("recipient_name").Trim()), year, kinds, elements, location);
            }
        }

        var events = new List<DyadEvent>();
        foreach (var (key, byYear) in years)
        {
            foreach (var run in Runs(byYear))
            {
                var who = run.Elements ? $"Elements of the {countries.Name(key.Supporter)} state" : countries.Name(key.Supporter);
                var other = countries.Name(key.Other);
                var title = key.Type == EventTypes.Support
                    ? $"{countries.Name(key.Supporter)} supported {other}'s government"
                    : $"{countries.Name(key.Supporter)} backed {Plain(key.Partner)} against {other}";
                var detail = key.Type == EventTypes.Support
                    ? $"{who} supported the government of {other} in its conflict with {Plain(key.Partner)} ({run.Location})."
                    : $"{who} supported {Plain(key.Partner)} in its conflict with the government of {other} ({run.Location}).";
                if (run.Kinds.Count > 0)
                {
                    detail += " Kind of support: " + string.Join(", ", run.Kinds.OrderBy(k => k)) + ".";
                }

                events.Add(DyadEvent.Create(key.Supporter, key.Other, key.Type, Source,
                    new DatePart(run.Start, null, null), new DatePart(run.End, null, null), title, detail));
            }
        }

        return events;
    }

    private static string Plain(string actor) =>
        actor.StartsWith("Government of ", StringComparison.Ordinal) ? "the government of " + actor["Government of ".Length..] : actor;

    private static IEnumerable<(int Start, int End, HashSet<string> Kinds, bool Elements, string Location)> Runs(
        SortedDictionary<int, (HashSet<string> Kinds, bool Elements, string Location)> byYear)
    {
        (int Start, int End, HashSet<string> Kinds, bool Elements, string Location)? cur = null;
        foreach (var (year, y) in byYear)
        {
            if (cur is { } c && c.End == year - 1)
            {
                c.Kinds.UnionWith(y.Kinds);
                cur = (c.Start, year, c.Kinds, c.Elements && y.Elements, c.Location);
                continue;
            }

            if (cur is not null)
            {
                yield return cur.Value;
            }

            cur = (year, year, new HashSet<string>(y.Kinds), y.Elements, y.Location);
        }

        if (cur is not null)
        {
            yield return cur.Value;
        }
    }
}
