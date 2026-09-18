using Alza.Delivery.DataMigrations;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile("appsettings.Development.json", true, true)
    .AddCommandLine(args)
    .AddEnvironmentVariables()
    .Build();

var runner = new MigrationRunner(config);
await runner.Run();
