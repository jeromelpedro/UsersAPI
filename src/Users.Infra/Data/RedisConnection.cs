using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Users.Domain.Utils;

namespace Users.Infra.Data;

public class RedisConnection
{
    private readonly Lazy<IConnectionMultiplexer> _connection;

    public RedisConnection(IConfiguration configuration)
    {
        _connection = new Lazy<IConnectionMultiplexer>(() =>
        {
            var host = configuration.GetConfigValue("Redis:Host", "localhost") ?? "localhost";
            var portValue = configuration.GetConfigValue("Redis:Port", "6379");
            var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 6379;

            var options = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
            };

            options.EndPoints.Add(host, port);

            return ConnectionMultiplexer.Connect(options);
        });
    }

    public IConnectionMultiplexer GetConnection() => _connection.Value;
}