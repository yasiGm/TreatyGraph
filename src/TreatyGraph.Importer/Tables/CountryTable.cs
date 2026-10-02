using TreatyGraph.Importer.Etl;

namespace TreatyGraph.Importer.Tables;

public sealed class CountryTable : Table<CountryTable.Row>
{
    public sealed record Row(int Ccode, string Abbr, string Name);

    public override string Name => "Country";

    protected override IEnumerable<Row> Rows(ImportData data) =>
        data.Countries.All.Select(c => new Row(c.Ccode, Trunc(c.Abbr, 8), Trunc(c.Name, 100)));
}
