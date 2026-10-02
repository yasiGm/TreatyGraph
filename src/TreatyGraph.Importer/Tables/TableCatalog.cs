namespace TreatyGraph.Importer.Tables;

/// <summary>
/// Every table, in foreign-key dependency order: a table comes after the tables it references.
/// Tables are created and loaded in this order and dropped in reverse, so this list replaces the
/// old numeric file-name prefixes. To add a table: sql/Tables/{Name}.sql + a Table class + one line here.
/// </summary>
public static class TableCatalog
{
    public static readonly IReadOnlyList<ITable> All = new ITable[]
    {
        new CountryTable(),
        new CountrySpanTable(),
        new LeaderTable(),
        new DyadEventTable(),
        new CountryWarTable(),
        new CivilWarTable(),
    };
}
