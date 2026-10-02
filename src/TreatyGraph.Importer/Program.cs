using TreatyGraph.Core.Etl;
using TreatyGraph.Importer;
using TreatyGraph.Importer.Database;
using TreatyGraph.Importer.Etl;
using TreatyGraph.Importer.Tables;
using Microsoft.Data.SqlClient;

try
{
    var options = ImportOptions.Parse(args);

    Console.WriteLine("1/4  Reading CSV files ...");
    var data = ImportDataReader.Read(new SourceFiles(options.DataDir));
    Console.WriteLine($"     {data.Countries.All.Count} countries, {data.DyadEvents.Count} pair events, {data.CountryWars.Count} war participations, {data.CivilWars.Count} civil-war rows, {data.Leaders.Count} leaders");

    Console.WriteLine("2/4  Creating database and tables ...");
    if (options.CreateDatabase)
    {
        await SqlScriptRunner.EnsureDatabaseAsync(options.ConnectionString);
    }

    await using var connection = new SqlConnection(options.ConnectionString);
    await connection.OpenAsync();
    await SchemaBuilder.RecreateTablesAsync(connection, options.SqlDir, TableCatalog.All);

    Console.WriteLine("3/4  Loading tables ...");
    foreach (var table in TableCatalog.All)
    {
        await BulkLoader.CopyAsync(connection, $"dbo.{table.Name}", table.BuildData(data));
    }

    Console.WriteLine("4/4  Creating stored procedures ...");
    await SchemaBuilder.CreateProceduresAsync(connection, options.SqlDir);

    Console.WriteLine("Done.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("Import failed: " + ex.Message);
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}
