namespace TreatyGraph.Importer;

public sealed record ImportOptions(string DataDir, string SqlDir, string ConnectionString, bool CreateDatabase)
{
    private const string DefaultConnection =
        "Server=localhost,1433;Database=TreatyGraph;User Id=sa;Password=Your_strong_Passw0rd!;TrustServerCertificate=True;Encrypt=False";

    public static ImportOptions Parse(string[] args)
    {
        var dataDir = "data";
        var sqlDir = "sql";
        var connection = Environment.GetEnvironmentVariable("CR_CONNECTION") ?? DefaultConnection;
        var createDb = true;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--data" when i + 1 < args.Length:
                    dataDir = args[++i];
                    break;
                case "--sql" when i + 1 < args.Length:
                    sqlDir = args[++i];
                    break;
                case "--connection" when i + 1 < args.Length:
                    connection = args[++i];
                    break;
                case "--no-create-db":
                    createDb = false;
                    break;
                default:
                    throw new ArgumentException(
                        "Unknown argument: " + args[i] +
                        "\nUsage: importer [--data <dir>] [--sql <dir>] [--connection <string>] [--no-create-db]");
            }
        }

        return new ImportOptions(dataDir, sqlDir, connection, createDb);
    }
}
