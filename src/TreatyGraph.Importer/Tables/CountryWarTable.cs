using TreatyGraph.Importer.Etl;

namespace TreatyGraph.Importer.Tables;

public sealed class CountryWarTable : Table<CountryWarTable.Row>
{
    public sealed record Row(
        int Ccode,
        string WarName,
        int StartYear,
        int? StartMonth,
        int? StartDay,
        int EndYear,
        int? EndMonth,
        int? EndDay,
        int SortKey,
        string? Result,
        bool IsInitiator,
        string Opponents,
        string Allies,
        int? BattleDeaths);

    public override string Name => "CountryWar";

    protected override IEnumerable<Row> Rows(ImportData data) =>
        data.CountryWars.Select(w => new Row(
            w.Ccode,
            Trunc(w.Name, 150),
            w.Start.Year,
            w.Start.Month,
            w.Start.Day,
            w.End.Year,
            w.End.Month,
            w.End.Day,
            w.Start.Key(),
            w.Result,
            w.Initiator,
            Trunc(string.Join("; ", w.Opponents), 1000),
            Trunc(string.Join("; ", w.Allies), 1000),
            w.BattleDeaths));
}
