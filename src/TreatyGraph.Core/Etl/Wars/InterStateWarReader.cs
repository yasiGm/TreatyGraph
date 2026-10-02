using TreatyGraph.Core.Csv;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Wars;

/// <summary>
/// Parses COW Inter-State War Data v4.0 into wars and participants. Shared by
/// <see cref="InterStateWarRule"/> (pair events) and <see cref="CountryWarRule"/> (profile rows).
/// </summary>
public static class InterStateWarReader
{
    public static List<InterStateWar> FromRows(IEnumerable<Dictionary<string, string>> rows)
    {
        var wars = new List<InterStateWar>();

        foreach (var war in rows.GroupBy(r => r.Get("WarNum")))
        {
            var rawName = war.First().Get("WarName").Trim();
            var title = rawName.Contains("war", StringComparison.OrdinalIgnoreCase) ? rawName : rawName + " War";

            var participants = new List<WarParticipant>();
            foreach (var r in war)
            {
                var code = Num.ParseInt(r.Get("ccode"));
                if (code is null)
                {
                    continue;
                }

                var periods = new List<(DatePart, DatePart)>();
                foreach (var i in new[] { "1", "2" })
                {
                    var s = DatePart.Clean(r.Get("StartYear" + i), r.Get("StartMonth" + i), r.Get("StartDay" + i));
                    var e = DatePart.Clean(r.Get("EndYear" + i), r.Get("EndMonth" + i), r.Get("EndDay" + i));
                    if (s is not null && e is not null)
                    {
                        periods.Add((s.Value, e.Value));
                    }
                }

                participants.Add(new WarParticipant(
                    code.Value,
                    r.Get("Side").Trim(),
                    periods,
                    Num.ParseInt(r.Get("Outcome")),
                    Num.ParseInt(r.Get("Initiator")),
                    Num.ParseInt(r.Get("BatDeath")) ?? -9));
            }

            wars.Add(new InterStateWar(war.Key, title, participants));
        }

        return wars;
    }
}
