using Alza.Delivery.InfrastructureLayer.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alza.Delivery.InfrastructureLayer.Extensions;

public static class ServicesExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var sqlConfig = configuration.GetSection("SqlDatabase").Get<SqlDatabaseConfig>()
            ?? throw new InvalidOperationException("SqlDatabase configuration section is missing or invalid.");

        services.AddDbContext<DeliveryDbContext>(options =>
            options.UseNpgsql(sqlConfig.ToConnectionString(), npgsql =>
                npgsql.MaxBatchSize(int.MaxValue)));

        return services;    
    }
}