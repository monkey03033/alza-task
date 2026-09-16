namespace Alza.Delivery;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<AlzaBoxVanAssignmentsJob>();

        var host = builder.Build();
        host.Run();
    }
}