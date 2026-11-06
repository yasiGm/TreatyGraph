using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Etl.Countries;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Wars;

/// <summary>
/// UCDP Candidate Events (preliminary, released monthly; Hegre et al. 2020). It covers the current year,
/// which the yearly UCDP/PRIO conflict file does not have yet. Only state-based events (type_of_violence = 1)
/// where both sides are governments are used; they are grouped by conflict and by pair of countries:
/// "war" for governments on opposing sides, "coalition" for governments on the same side.
/// The data are preliminary and may change when UCDP publishes the final yearly release.
/// </summary>
public sealed class UcdpCandidateRule : IEtlRule<DyadEvent>
{
    public const string Source = "UCDP Candidate Events (preliminary)";

    /// <summary>Events before this year are left to the final UCDP/PRIO conflict file.</summary>
    public const int FirstYear = 2026;

    public IReadOnlyList<DyadEvent> Load(SourceFiles files, CountryDirectory countries) =>
        files.UcdpCandidate is string path
            ? FromRows(CsvReader.ReadDictionaries(path), countries)
            : Array.Empty<DyadEvent>();

    public List<DyadEvent> FromRows(IEnumerable<Dictionary<string, string>> rows, CountryDirectory countries)
    {
        var byName = countries.All.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Max(c => c.Ccode), StringComparer.OrdinalIgnoreCase);

        var groups = new Dictionary<(string Conflict, int A, int B, bool SameSide), (DateOnly First, DateOnly Last, int Events, int Deaths)>();
        var latest = DateOnly.MinValue;

        foreach (var r in rows)
        {
            if (r.Get("type_of_violence").Trim() != "1"
                || DateOf(r.Get("date_start")) is not DateOnly start
                || DateOf(r.Get("date_end")) is not DateOnly end
                || end.Year < FirstYear)
            {
                continue;
            }

            if (start.Year < FirstYear)
            {
                start = new DateOnly(FirstYear, 1, 1);
            }

            var sideA = Governments(r.Get("gwnoa"), r.Get("side_a"), start.Year, countries, byName);
            var sideB = Governments(r.Get("gwnob"), r.Get("side_b"), start.Year, countries, byName);
            if (sideA.Count == 0 || sideB.Count == 0)
            {
                continue; // a rebel group or non-state actor on one side: not between two countries
            }

            latest = end > latest ? end : latest;
            var conflict = r.Get("conflict_new_id").Trim();
            var deaths = Num.ParseInt(r.Get("best")) ?? 0;

            void Add(int a, int b, bool same)
            {
                if (a == b)
                {
                    return;
                }

                var key = (conflict, Math.Min(a, b), Math.Max(a, b), same);
                groups[key] = groups.TryGetValue(key, out var g)
                    ? (start < g.First ? start : g.First, end > g.Last ? end : g.Last, g.Events + 1, g.Deaths + deaths)
                    : (start, end, 1, deaths);
            }

            foreach (var a in sideA)
            {
                foreach (var b in sideB)
                {
                    Add(a, b, false);
                }
            }

            foreach (var side in new[] { sideA, sideB })
            {
                for (var i = 0; i < side.Count; i++)
                {
                    for (var j = i + 1; j < side.Count; j++)
                    {
                        Add(side[i], side[j], true);
                    }
                }
            }
        }

        var events = new List<DyadEvent>();
        foreach (var ((_, lo, hi, same), g) in groups.OrderBy(x => x.Value.First))
        {
            var title = same
                ? $"{countries.Name(lo)} and {countries.Name(hi)} fighting on the same side ({g.First.Year})"
                : $"{countries.Name(lo)} – {countries.Name(hi)} armed conflict ({g.First.Year})";
            var detail = $"UCDP Candidate Events (preliminary): {g.Events} recorded event(s) between {g.First:yyyy-MM-dd} and {g.Last:yyyy-MM-dd}, "
                + $"about {g.Deaths:N0} deaths in total. Figures may change in UCDP's final yearly release.";
            var ongoing = latest.Year == g.Last.Year && latest.Month == g.Last.Month;

            events.Add(DyadEvent.Create(lo, hi, same ? EventTypes.Coalition : EventTypes.War, Source,
                new DatePart(g.First.Year, g.First.Month, g.First.Day),
                new DatePart(g.Last.Year, g.Last.Month, g.Last.Day),
                title, detail, ongoing));
        }

        return events;
    }

    // "2026-02-28" or "2026-02-28 00:00:00.000" -> 2026-02-28
    private static DateOnly? DateOf(string text)
    {
        var t = text.Trim();
        return t.Length >= 10 && DateOnly.TryParseExact(t[..10], "yyyy-MM-dd", out var d) ? d : null;
    }

    // A side as COW codes: from the Gleditsch-Ward code column when it holds numbers, otherwise from
    // "Government of X" names. Anything that is not a government gives an empty list.
    private static List<int> Governments(string codes, string names, int year, CountryDirectory countries, Dictionary<string, int> byName)
    {
        var result = codes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Num.ParseInt(s))
            .Where(c => c is not null)
            .Select(c => GleditschWard.ToCow(c!.Value, year))
            .Where(countries.Contains)
            .ToList();
        if (result.Count > 0)
        {
            return result.Distinct().ToList();
        }

        foreach (var part in names.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!part.StartsWith("Government of ", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var name = part["Government of ".Length..].Trim();
            var plain = name.Contains('(') ? name[..name.IndexOf('(')].Trim() : name; // "Russia (Soviet Union)" -> "Russia"
            if (byName.TryGetValue(name, out var code) || byName.TryGetValue(plain, out code))
            {
                result.Add(code);
            }
        }

        return result.Distinct().ToList();
    }
}
