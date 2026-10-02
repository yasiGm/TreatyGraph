using TreatyGraph.Importer.Etl;

namespace TreatyGraph.Importer.Tables;

public sealed class DyadEventTable : Table<DyadEventTable.Row>
{
    public sealed record Row(
        int CcodeLo,
        int CcodeHi,
        string EventType,
        string Source,
        int StartYear,
        int? StartMonth,
        int? StartDay,
        int? EndYear,
        int? EndMonth,
        int? EndDay,
        bool IsOngoing,
        bool IsLeftCensored,
        int SortKey,
        string Title,
        string Detail);

    public override string Name => "DyadEvent";

    protected override IEnumerable<Row> Rows(ImportData data) =>
        data.DyadEvents.Select(e => new Row(
            e.CcodeLo,
            e.CcodeHi,
            e.Type,
            Trunc(e.Source, 40),
            e.Start.Year,
            e.Start.Month,
            e.Start.Day,
            e.End?.Year,
            e.End?.Month,
            e.End?.Day,
            e.Ongoing,
            e.LeftCensored,
            e.SortKey,
            Trunc(e.Title, 200),
            Trunc(e.Detail, 1000)));
}
