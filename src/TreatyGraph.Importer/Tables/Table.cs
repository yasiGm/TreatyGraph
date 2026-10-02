using System.Data;
using TreatyGraph.Importer.Database;
using TreatyGraph.Importer.Etl;

namespace TreatyGraph.Importer.Tables;

/// <summary>
/// Base class for a table whose rows are the record <typeparamref name="TRow"/>. The record's
/// property names must match the table's column names one-to-one (see <see cref="DataTableBuilder"/>).
/// </summary>
public abstract class Table<TRow> : ITable
{
    public abstract string Name { get; }

    public DataTable BuildData(ImportData data) => DataTableBuilder.From(Rows(data));

    protected abstract IEnumerable<TRow> Rows(ImportData data);

    protected static string Trunc(string value, int max) => value.Length <= max ? value : value[..max];
}
