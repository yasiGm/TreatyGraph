using System.Data;
using Microsoft.Data.SqlClient;

namespace TreatyGraph.Importer.Database;

/// <summary>Bulk-copies a DataTable (built by <see cref="DataTableBuilder"/>) into a SQL Server table.</summary>
public static class BulkLoader
{
    public static async Task CopyAsync(SqlConnection connection, string table, DataTable data)
    {
        using var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.CheckConstraints, null)
        {
            DestinationTableName = table,
            BatchSize = 5000,
            BulkCopyTimeout = 0,
        };

        foreach (DataColumn column in data.Columns)
        {
            bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        await bulk.WriteToServerAsync(data);
    }
}
