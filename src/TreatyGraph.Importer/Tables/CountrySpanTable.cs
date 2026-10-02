using TreatyGraph.Importer.Etl;

namespace TreatyGraph.Importer.Tables;

public sealed class CountrySpanTable : Table<CountrySpanTable.Row>
{
    public sealed record Row(int Ccode, int StartYear, int EndYear);

    public override string Name => "CountrySpan";

    protected override IEnumerable<Row> Rows(ImportData data) =>
        data.Countries.All.SelectMany(c => c.Spans.Select(s => new Row(c.Ccode, s.Start, s.End)));
}
