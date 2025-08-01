using Microsoft.Data.SqlClient;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
