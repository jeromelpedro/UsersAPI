using Microsoft.Extensions.Configuration;

namespace Users.Domain.Utils
{
	public static class ConfigurationExtensions
	{
		public static string? GetConfigValue(this IConfiguration configuration, string key, string? defaultValue = null)
		{
			if (configuration is null)
				return defaultValue;

			var value = configuration[key];

			if (string.IsNullOrWhiteSpace(value))
			{
				var envKey = key.Replace(":", "__");
				var env = Environment.GetEnvironmentVariable(envKey, EnvironmentVariableTarget.Process);
				return string.IsNullOrWhiteSpace(env) ? defaultValue : env;
			}

			return value ?? defaultValue;
		}
	}
}
