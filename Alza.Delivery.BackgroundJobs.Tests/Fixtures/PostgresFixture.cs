using Alza.Delivery.InfrastructureLayer;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Alza.Delivery.BackgroundJobs.Tests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("alza_delivery_test")
        .WithUsername("TEST")
        .WithPassword("TEST")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var dbContext = CreateDbContext();

        for (var attempt = 1; !await dbContext.Database.CanConnectAsync(); attempt++)
        {
            if (attempt >= 10)
                throw new TimeoutException("Postgres test container did not become reachable.");

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        await dbContext.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public DeliveryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DeliveryDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new DeliveryDbContext(options);
    }
}
