using System.Data;
using Frametric.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Frametric.Infrastructure.Persistence;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "DefaultConnection connection string is missing.");
    }

    public IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
