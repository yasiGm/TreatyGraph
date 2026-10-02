using Microsoft.Data.SqlClient;

namespace TreatyGraph.Api.Data;

/// <summary>Tiny connection factory so the repository does not know about connection strings.</summary>
public sealed class Db
{
    private readonly string _connectionString;

    public Db(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<SqlConnection> OpenAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
