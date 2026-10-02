using TreatyGraph.Importer.Tables;
using Microsoft.Data.SqlClient;

namespace TreatyGraph.Importer.Database;

/// <summary>Drops and recreates the tables in <see cref="TableCatalog"/>, then (re)creates the procedures.</summary>
public static class SchemaBuilder
{
    public static async Task RecreateTablesAsync(SqlConnection connection, string sqlDir, IReadOnlyList<ITable> tables)
    {
        var tablesDir = Path.Combine(sqlDir, "Tables");
        EnsureEveryScriptIsRegistered(tablesDir, tables);

        foreach (var table in tables.Reverse())
        {
            await SqlScriptRunner.ExecuteAsync(connection, $"DROP TABLE IF EXISTS dbo.[{table.Name}];");
        }

        foreach (var table in tables)
        {
            await SqlScriptRunner.RunAsync(connection, Path.Combine(tablesDir, table.Name + ".sql"));
        }
    }

    public static Task CreateProceduresAsync(SqlConnection connection, string sqlDir) =>
        SqlScriptRunner.RunDirectoryAsync(connection, Path.Combine(sqlDir, "Procedures"));

    // A table script without a matching class would silently never be created - fail loudly instead.
    private static void EnsureEveryScriptIsRegistered(string tablesDir, IReadOnlyList<ITable> tables)
    {
        var registered = tables.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var orphans = Directory.GetFiles(tablesDir, "*.sql")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !registered.Contains(name!))
            .ToList();

        if (orphans.Count > 0)
        {
            throw new InvalidOperationException(
                $"sql/Tables has scripts with no table class in TableCatalog: {string.Join(", ", orphans)}");
        }
    }
}
