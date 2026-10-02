using TreatyGraph.Importer.Etl;

namespace TreatyGraph.Importer.Tables;

public sealed class LeaderTable : Table<LeaderTable.Row>
{
    public sealed record Row(int Ccode, string Name, DateTime StartDate, DateTime EndDate, string ExitType);

    public override string Name => "Leader";

    protected override IEnumerable<Row> Rows(ImportData data) =>
        data.Leaders.Select(l => new Row(
            l.Ccode,
            Trunc(l.Name, 120),
            l.Start.ToDateTime(TimeOnly.MinValue),
            l.End.ToDateTime(TimeOnly.MinValue),
            Trunc(l.ExitType, 60)));
}
