namespace Alza.Delivery;

public class AlzaBoxVanAssignmentsJob : BackgroundService
{
    private readonly ILogger<AlzaBoxVanAssignmentsJob> _logger;

    public AlzaBoxVanAssignmentsJob(ILogger<AlzaBoxVanAssignmentsJob> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}