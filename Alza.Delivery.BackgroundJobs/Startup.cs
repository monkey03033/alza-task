using Alza.Delivery.BackgroundJobs.Planning;
using Alza.Delivery.InfrastructureLayer.Extensions;

namespace Alza.Delivery.BackgroundJobs;

public class Startup
{

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment Environment { get; }

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHostedService<AlzaBoxVanAssignmentsJob>();

        services.AddTransient<IPlanningService, PlanningService>();

        services.Configure<VanConfig>(Configuration.GetSection(nameof(VanConfig)));

        services.AddInfrastructureServices(Configuration);

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        //
    }
}