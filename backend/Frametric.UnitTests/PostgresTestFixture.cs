using System;
using System.Threading.Tasks;
using Frametric.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Frametric.UnitTests;

public class PostgresTestFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    public string ConnectionString => Container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        // Run EF Core migrations to prepare the database schema
        var options = new DbContextOptionsBuilder<FrametricDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using var context = new FrametricDbContext(options);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
