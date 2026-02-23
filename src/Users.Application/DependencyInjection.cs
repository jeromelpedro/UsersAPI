using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Interfaces;
using Users.Application.Services;
using Users.Domain.Dto;
using Users.Domain.Interfaces;
using Users.Domain.Interfaces.MessageBus;
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
					Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"),
					sqlOptions => sqlOptions.EnableRetryOnFailure()
				);

				// Register interceptor from DI so it can access HttpContext and logger
				var interceptor = serviceProvider.GetService<Users.Infra.Data.AuditSaveChangesInterceptor>();
				if (interceptor != null)
					options.AddInterceptors(interceptor);
			});

			// Configurar RabbitMQ a partir das variáveis de ambiente
			services.Configure<RabbitMqSettings>(options =>
			{
				options.HostName = Environment.GetEnvironmentVariable("RabbitMq__HostName") ?? "localhost";
				options.Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMq__Port") ?? "5672");
				options.Username = Environment.GetEnvironmentVariable("RabbitMq__UserName") ?? "guest";
				options.Password = Environment.GetEnvironmentVariable("RabbitMq__Password") ?? "guest";
				options.ExchangeName = Environment.GetEnvironmentVariable("RabbitMq__ExchangeName") ?? "cloudgames.topic";
			});

			services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
		}
	}
}
