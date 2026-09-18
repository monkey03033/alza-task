using Npgsql;

namespace Alza.Delivery.InfrastructureLayer.Configurations;

public class SqlDatabaseConfig
{
    public string Host { get; set; } = default!;
    public int Port { get; set; } = 5432;
    public string Database { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;

    public string ToConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Host,
            Port = Port,
            Database = Database,
            Username = Username,
            Password = Password
        };
        return builder.ConnectionString;
    }
}
