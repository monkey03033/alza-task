using System.Reflection;
using Alza.Delivery.InfrastructureLayer;
using Alza.Delivery.InfrastructureLayer.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Alza.Delivery.DataMigrations.Design;

public class DeliveryTimeDbContextFactory : IDesignTimeDbContextFactory<DeliveryDbContext>
{
    public DeliveryDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile("appsettings.Development.json", true, true)
            .AddCommandLine(args)
            .AddEnvironmentVariables()
            .Build();

        var sqlConfig = config.GetSection("SqlDatabase").Get<SqlDatabaseConfig>()
            ?? throw new InvalidOperationException("SqlDatabase configuration section is missing or invalid.");

        var connectionString = sqlConfig.ToConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<DeliveryDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            opts =>
            {
                opts.MigrationsAssembly(Assembly.GetAssembly(typeof(DeliveryTimeDbContextFactory))?.GetName().Name);
            });

        return new DeliveryDbContext(optionsBuilder.Options);
    }
}
