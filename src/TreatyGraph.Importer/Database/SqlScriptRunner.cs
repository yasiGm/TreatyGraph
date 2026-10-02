using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace TreatyGraph.Importer.Database;

public static class SqlScriptRunner
{
    private static readonly Regex GoSeparator =
        new(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>Runs a T-SQL script, splitting it into batches on lines that contain only GO.</summary>
    public static async Task RunAsync(SqlConnection connection, string path)
    {
        var script = await File.ReadAllTextAsync(path);
        foreach (var batch in GoSeparator.Split(script))
        {
            if (string.IsNullOrWhiteSpace(batch))
            {
                continue;
            }

            await ExecuteAsync(connection, batch);
        }
    }

    /// <summary>
    /// Runs every *.sql file in a folder. Only for scripts whose order does not matter
    /// (e.g. sql/Procedures, one file per procedure); tables are ordered by TableCatalog instead.
    /// </summary>
    public static async Task RunDirectoryAsync(SqlConnection connection, string directory)
    {
        var files = Directory.GetFiles(directory, "*.sql").OrderBy(f => f, StringComparer.Ordinal);
        foreach (var file in files)
        {
            await RunAsync(connection, file);
        }
    }

    public static async Task ExecuteAsync(SqlConnection connection, string sql)
    {
        await using var command = new SqlCommand(sql, connection) { CommandTimeout = 300 };
        await command.ExecuteNonQueryAsync();
    }

    public static async Task EnsureDatabaseAsync(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var database = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException("The connection string must name a database (Database=...).");
        }

        builder.InitialCatalog = "master";
        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        const string sql = @"
IF DB_ID(@name) IS NULL
BEGIN
    DECLARE @stmt nvarchar(400) = N'CREATE DATABASE ' + QUOTENAME(@name);
    EXEC (@stmt);
END";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", database);
        await command.ExecuteNonQueryAsync();
    }
}
