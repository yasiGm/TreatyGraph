using TreatyGraph.Importer.Etl;

namespace TreatyGraph.Importer.Tables;

public sealed class CivilWarTable : Table<CivilWarTable.Row>
{
    public sealed record Row(
        int Ccode,
        string WarName,
        int StartYear,
        int? StartMonth,
        int? StartDay,
        int? EndYear,
        int? EndMonth,
        int? EndDay,
        int SortKey,
        string Sides,
        bool IsInternationalized);

    public override string Name => "CivilWar";

    protected override IEnumerable<Row> Rows(ImportData data) =>
        data.CivilWars.Select(w => new Row(
            w.Ccode,
            Trunc(w.Name, 150),
            w.Start.Year,
            w.Start.Month,
            w.Start.Day,
            w.End?.Year,
            w.End?.Month,
            w.End?.Day,
            w.Start.Key(),
            Trunc(w.Sides, 400),
            w.Internationalized));
}
