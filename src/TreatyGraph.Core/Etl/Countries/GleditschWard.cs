using System.Text;
using TreatyGraph.Core.Model;

namespace TreatyGraph.Core.Etl.Countries;

/// <summary>
/// The Gleditsch and Ward (G&amp;W) list of independent states since 1816 (Gleditsch &amp; Ward 1999,
/// "Interstate System Membership: A Revised List of the Independent States since 1816").
/// G&amp;W codes match the COW codes for almost every state. The database keys countries by COW code,
/// so the few codes that differ are translated here. UCDP uses G&amp;W codes too.
/// The list can replace the COW state list when that file is missing (see <see cref="CountryLoader"/>).
/// </summary>
public static class GleditschWard
{
    /// <summary>The G&amp;W list's last year; a state whose end date falls in it is treated as still independent.</summary>
    public const int ListEndYear = 2017;

    /// <summary>The COW code for a G&amp;W code in a given year.</summary>
    public static int ToCow(int gwCode, int year) => gwCode switch
    {
        260 when year >= 1991 => 255, // G&W keeps 260 for unified Germany; COW goes back to 255 from 1990
        340 => 345,                   // Serbia: G&W 340, COW 345 (continuing Yugoslavia)
        678 when year >= 1990 => 679, // unified Yemen: G&W keeps 678, COW uses 679 from 1990
        970 => 946,                   // Kiribati
        971 => 970,                   // Nauru
        972 => 955,                   // Tonga
        973 => 947,                   // Tuvalu
        _ => gwCode,
    };

    /// <summary>Reads iisystem.dat and microstatessystem.dat (tab separated: code, abbreviation, name, dd:mm:yyyy, dd:mm:yyyy).</summary>
    public static CountryDirectory Load(IEnumerable<string> paths, int currentYear)
    {
        var latin1 = Encoding.Latin1;
        var lines = paths.Where(File.Exists).SelectMany(p => File.ReadLines(p, latin1));
        return FromLines(lines, currentYear);
    }

    public static CountryDirectory FromLines(IEnumerable<string> lines, int currentYear)
    {
        var order = new List<int>();
        var info = new Dictionary<int, (string Abbr, string Name)>();
        var spans = new Dictionary<int, List<YearSpan>>();

        void Add(int code, string abbr, string name, int start, int end)
        {
            if (end < start)
            {
                return;
            }

            if (!spans.ContainsKey(code))
            {
                order.Add(code);
                info[code] = (abbr, name);
                spans[code] = new List<YearSpan>();
            }

            spans[code].Add(new YearSpan(start, end));
        }

        foreach (var line in lines)
        {
            var f = line.TrimEnd('\r').Split('\t');
            if (f.Length < 5 || Num.ParseInt(f[0]) is not int gw)
            {
                continue;
            }

            var start = YearOf(f[3]);
            var end = YearOf(f[4]);
            if (start is null || end is null)
            {
                continue;
            }

            var last = end.Value >= ListEndYear ? currentYear : end.Value;
            var (abbr, name) = (f[1].Trim(), f[2].Trim());

            // Germany and Yemen change COW code in 1990, so their G&W rows are split there.
            if (gw is 260 or 678 && start < 1990 && last >= 1990)
            {
                Add(gw, abbr, name, start.Value, 1990);
                Add(ToCow(gw, 1991), gw == 260 ? "GMY" : "YEM", gw == 260 ? "Germany" : "Yemen", 1990, last);
                continue;
            }

            Add(ToCow(gw, start.Value), abbr, name, start.Value, last);
        }

        return new CountryDirectory(order.Select(c => new Country(c, info[c].Abbr, info[c].Name, spans[c])));
    }

    // "dd:mm:yyyy" -> yyyy
    private static int? YearOf(string date)
    {
        var parts = date.Trim().Split(':');
        return parts.Length == 3 ? Num.ParseInt(parts[2]) : null;
    }
}
