using System.Data;
using TreatyGraph.Core.Model;
using Dapper;

namespace TreatyGraph.Api.Data;

public sealed class Repository
{
    private static readonly string[] Separator = { "; " };
    private readonly Db _db;

    public Repository(Db db)
    {
        _db = db;
    }

    // "1921", "1921-02" or "1921-02-26", depending on how much the source knows
    public static string Fmt(int year, int? month, int? day) =>
        new DatePart(year, month, day).ToString();

    private static string? FmtOpt(int? year, int? month, int? day) =>
        year is null ? null : Fmt(year.Value, month, day);

    // Every query is a stored procedure in sql/Procedures (one file per procedure).
    private static Task<IEnumerable<T>> QueryAsync<T>(IDbConnection conn, string procedure, object? args = null) =>
        conn.QueryAsync<T>(procedure, args, commandType: CommandType.StoredProcedure);

    private static string[] Split(string value) =>
        string.IsNullOrEmpty(value) ? Array.Empty<string>() : value.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

    // ------------------------------------------------------------------ meta
    public async Task<MetaDto> GetMetaAsync()
    {
        await using var conn = await _db.OpenAsync();

        var countries = (await QueryAsync<CountryRow>(conn, "dbo.usp_GetCountries")).ToList();

        var spans = (await QueryAsync<SpanRow>(conn, "dbo.usp_GetCountrySpans")).ToLookup(s => s.Ccode);

        var withLeaders = (await QueryAsync<int>(conn, "dbo.usp_GetCountriesWithLeaders")).ToHashSet();

        var list = countries
            .Select(c => new CountryMetaDto(
                c.Ccode,
                c.Abbr,
                c.Name,
                spans[c.Ccode].Select(s => new[] { s.StartYear, s.EndYear }).ToArray(),
                withLeaders.Contains(c.Ccode)))
            .ToList();

        return new MetaDto(list, Sources.Coverage);
    }

    // ------------------------------------------------------------------ pair
    public async Task<IReadOnlyList<EventDto>> GetPairAsync(int a, int b)
    {
        await using var conn = await _db.OpenAsync();

        var rows = await QueryAsync<PairEventRow>(conn, "dbo.usp_GetPairTimeline", new { CcodeA = (short)a, CcodeB = (short)b });

        return rows
            .Select(r => new EventDto(
                r.EventType,
                r.Source,
                Fmt(r.StartYear, r.StartMonth, r.StartDay),
                FmtOpt(r.EndYear, r.EndMonth, r.EndDay),
                r.IsOngoing,
                r.IsLeftCensored,
                r.SortKey,
                r.Title,
                r.Detail))
            .ToList();
    }

    // --------------------------------------------------------------- profile
    public async Task<CountryProfileDto?> GetProfileAsync(int ccode)
    {
        await using var conn = await _db.OpenAsync();
        var args = new { Ccode = (short)ccode };

        var country = (await QueryAsync<CountryRow>(conn, "dbo.usp_GetCountry", args)).SingleOrDefault();
        if (country is null)
        {
            return null;
        }

        var spans = (await QueryAsync<SpanRow>(conn, "dbo.usp_GetCountrySpans", args)).Select(s => new[] { s.StartYear, s.EndYear }).ToArray();

        var leaderRows = (await QueryAsync<LeaderRow>(conn, "dbo.usp_GetCountryLeaders", args)).ToList();
        var leaders = leaderRows.Count == 0
            ? null
            : (IReadOnlyList<LeaderDto>)leaderRows
                .Select(l => new LeaderDto(l.Name, l.StartDate.ToString("yyyy-MM-dd"), l.EndDate.ToString("yyyy-MM-dd"), l.ExitType))
                .ToList();

        var wars = (await QueryAsync<WarRow>(conn, "dbo.usp_GetCountryWars", args))
            .Select(w => new WarDto(
                w.WarName,
                Fmt(w.StartYear, w.StartMonth, w.StartDay),
                Fmt(w.EndYear, w.EndMonth, w.EndDay),
                w.SortKey,
                w.Result,
                w.IsInitiator,
                Split(w.Opponents),
                Split(w.Allies),
                w.BattleDeaths))
            .ToList();

        var civil = (await QueryAsync<CivilWarRow>(conn, "dbo.usp_GetCountryCivilWars", args))
            .Select(w => new CivilWarDto(
                w.WarName,
                Fmt(w.StartYear, w.StartMonth, w.StartDay),
                FmtOpt(w.EndYear, w.EndMonth, w.EndDay),
                w.SortKey,
                w.Sides,
                w.IsInternationalized))
            .ToList();

        var allianceRows = await QueryAsync<AllianceRow>(conn, "dbo.usp_GetCountryAlliances", args);

        var alliances = allianceRows
            .GroupBy(r => new { r.Partner, r.PartnerName })
            .Select(g => new AlliancePartnerDto(
                g.Key.Partner,
                g.Key.PartnerName,
                g.Select(r => new AllianceItemDto(
                    r.Title,
                    r.Source,
                    Fmt(r.StartYear, r.StartMonth, r.StartDay),
                    FmtOpt(r.EndYear, r.EndMonth, r.EndDay),
                    r.IsOngoing)).ToList()))
            .ToList();

        return new CountryProfileDto(country.Ccode, country.Abbr, country.Name, spans, leaders, wars, civil, alliances);
    }
}
