using System.Globalization;
using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Wars;

/// <summary>
/// COW Inter-State Wars -> one pair event per pair of participants: "war" when they were on
/// opposing sides, "coalition" when on the same side. The window is the overlap of their periods.
/// </summary>
public sealed class InterStateWarRule : IEtlRule<DyadEvent>
{
    public const string Source = "COW Inter-State Wars v4.0";

    public IReadOnlyList<DyadEvent> Load(SourceFiles files, CountryDirectory countries) =>
        FromRows(CsvReader.ReadDictionaries(files.InterStateWars), countries);

    public List<DyadEvent> FromRows(IEnumerable<Dictionary<string, string>> rows, CountryDirectory countries)
    {
        var events = new List<DyadEvent>();

        foreach (var war in InterStateWarReader.FromRows(rows))
        {
            var parts = war.Participants;
            for (var i = 0; i < parts.Count; i++)
            {
                for (var j = i + 1; j < parts.Count; j++)
                {
                    var p = parts[i];
                    var q = parts[j];
                    if (p.Ccode == q.Ccode)
                    {
                        continue;
                    }

                    foreach (var (s1, e1) in p.Periods)
                    {
                        foreach (var (s2, e2) in q.Periods)
                        {
                            var start = s1.Key() >= s2.Key() ? s1 : s2;
                            var end = e1.Key(true) <= e2.Key(true) ? e1 : e2;
                            if (end.Key(true) < start.Key())
                            {
                                continue;
                            }

                            var opposed = p.Side != q.Side;
                            var title = war.Title + (opposed ? " — on opposing sides" : " — on the same side");
                            var detail = $"War #{war.WarNum}. {Describe(p, countries)}; {Describe(q, countries)}.";
                            events.Add(DyadEvent.Create(
                                p.Ccode,
                                q.Ccode,
                                opposed ? EventTypes.War : EventTypes.Coalition,
                                Source,
                                start,
                                end,
                                title,
                                detail));
                        }
                    }
                }
            }
        }

        return events;
    }

    private static string Describe(WarParticipant x, CountryDirectory countries)
    {
        var text = countries.Name(x.Ccode) + (x.IsInitiator ? " (initiator)" : string.Empty);
        if (x.Result is not null)
        {
            text += ": " + x.Result;
        }

        if (x.Deaths > 0)
        {
            text += ", " + x.Deaths.ToString("N0", CultureInfo.InvariantCulture) + " battle deaths";
        }

        return text;
    }
}
