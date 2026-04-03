using StackExchange.Redis;

namespace Users.Infra.Data;

public class RedisConnection
{
    private readonly Lazy<IConnectionMultiplexer> _connection;

    public RedisConnection()
    {
        _connection = new Lazy<IConnectionMultiplexer>(() =>
        {
            var host = Environment.GetEnvironmentVariable("Redis__Host")
                ?? "localhost";

            var options = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
            };

            options.EndPoints.Add(host, 6379);

            return ConnectionMultiplexer.Connect(options);
        });
    }

    public IConnectionMultiplexer GetConnection() => _connection.Value;
}

public static class RedisProvider
{
    private static readonly Lazy<RedisConnection> _redis =
        new(() => new RedisConnection());

    public static IConnectionMultiplexer GetConnection() =>
        _redis.Value.GetConnection();
}