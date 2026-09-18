namespace Alza.Delivery.BackgroundJobs;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();

        var ct = new CancellationToken();
        await host.StartAsync(ct);

        await host.WaitForShutdownAsync(ct);
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => services.AddLogging())
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
