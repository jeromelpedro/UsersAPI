using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Interfaces;
using Users.Application.Services;
using Users.Domain.Interfaces;
using Users.Domain.Interfaces.MessageBus;
using Users.Domain.Utils;
using Users.Infra.Data;
using Users.Infra.MessageBus;
using Users.Infra.Repositories;


namespace Users.Application
{
	public static class DependencyInjection
	{
		public static void ResolveDependencyInjection(this IServiceCollection services, IConfiguration configuration)
		{
			ResolveDependencyInjectionConfigs(services, configuration);
			ResolveDependencyInjectionServices(services);
			ResolveDependencyInjectionRepositories(services);
		}

		private static void ResolveDependencyInjectionRepositories(IServiceCollection services)
		{
			services.AddTransient<IUsuarioRepository, UsuarioRepository>();
		}

		private static void ResolveDependencyInjectionServices(IServiceCollection services)
		{
			services.AddTransient<IUsuarioService, UsuarioService>();
			services.AddTransient<IJwtService, JwtService>();
		}

		private static void ResolveDependencyInjectionConfigs(IServiceCollection services, IConfiguration configuration)
		{
			services.AddScoped<JwtService>();

			services.AddScoped<AuditSaveChangesInterceptor>();

			services.AddDbContext<AppDbContext>((serviceProvider, options) =>
			{
				options.UseSqlServer(
					configuration.GetConnectionString("SqlConnection"),					
					sqlOptions => sqlOptions.EnableRetryOnFailure()
				);

				// Register interceptor from DI so it can access HttpContext and logger
				var interceptor = serviceProvider.GetService<Users.Infra.Data.AuditSaveChangesInterceptor>();
				if (interceptor != null)
					options.AddInterceptors(interceptor);
			});
			services.AddSingleton<IServiceBus, ServiceBus>();
			Security.Configure(configuration);
			services.AddServiceBus(configuration);
		}

		private static void AddServiceBus(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddSingleton<ServiceBusClient>(provider =>
			{
				var connectionString = configuration.GetConfigValue("ServiceBus:ConnectionString");
				return new ServiceBusClient(connectionString);
			});
		}
	}
}
