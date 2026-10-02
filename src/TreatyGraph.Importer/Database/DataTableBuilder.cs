using System.Data;
using System.Reflection;

namespace TreatyGraph.Importer.Database;

/// <summary>
/// Builds a <see cref="DataTable"/> from any sequence of flat records via reflection, so
/// bulk-loading a new table only needs one new table class (see the Tables/ folder) - no
/// hand-written DataTable column wiring.
/// A record's property names must match the destination table's column names exactly;
/// <see cref="BulkLoader"/> maps columns by name, so declaration order does not matter.
/// </summary>
public static class DataTableBuilder
{
    public static DataTable From<T>(IEnumerable<T> rows)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var table = new DataTable();
        foreach (var property in properties)
        {
            var columnType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            table.Columns.Add(property.Name, columnType);
        }

        foreach (var row in rows)
        {
            var values = new object[properties.Length];
            for (var i = 0; i < properties.Length; i++)
            {
                values[i] = properties[i].GetValue(row) ?? DBNull.Value;
            }

            table.Rows.Add(values);
        }

        return table;
    }
}
