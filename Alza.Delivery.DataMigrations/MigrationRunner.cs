using System.Reflection;
using Alza.Delivery.InfrastructureLayer;
using Alza.Delivery.InfrastructureLayer.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Alza.Delivery.DataMigrations;

public class MigrationRunner
{
    private readonly IConfiguration _configuration;

    public MigrationRunner(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task Run()
    {
        var sqlConfig = _configuration.GetSection("SqlDatabase").Get<SqlDatabaseConfig>()
            ?? throw new InvalidOperationException("SqlDatabase configuration section is missing or invalid.");

        var connectionString = sqlConfig.ToConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<DeliveryDbContext>()
            .UseNpgsql(connectionString,
                opts =>
                {
                    opts.MigrationsAssembly(Assembly.GetEntryAssembly()!.GetName().Name);
                    opts.CommandTimeout(600);
                });

        using var context = new DeliveryDbContext(optionsBuilder.Options);
        await context.Database.MigrateAsync();
    }
}
