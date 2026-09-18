using System.Diagnostics;
using System.Text.Json;
using Alza.Delivery.BackgroundJobs.Planning;

namespace Alza.Delivery.BackgroundJobs;

public sealed class AlzaBoxVanAssignmentsJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<AlzaBoxVanAssignmentsJob> _logger;

    public AlzaBoxVanAssignmentsJob(IServiceProvider serviceProvider,
        IHostApplicationLifetime applicationLifetime,
        ILogger<AlzaBoxVanAssignmentsJob> logger)
    {
        _serviceProvider = serviceProvider;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            using var scope = _serviceProvider.CreateScope();
            var planningService = scope.ServiceProvider.GetRequiredService<IPlanningService>();
            var result = await planningService.PlanDeliveries(stoppingToken);

            _logger.LogInformation("Planning completed in {ElapsedMilliseconds} ms.", stopwatch.ElapsedMilliseconds);

            await GenerateResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delivery planning failed.");
        }
        finally
        {
            _applicationLifetime.StopApplication();
        }
    }

    private static async Task GenerateResult(PlanningResult result)
    {
        var output = JsonSerializer.Serialize(new
        {
            result.TotalRevenue, result.Trips
        }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await File.WriteAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "planning-result.json"),
            output,
            CancellationToken.None);
    }
}