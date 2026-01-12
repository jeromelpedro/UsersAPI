using Microsoft.OpenApi.Models;

namespace Users.Api.Configurations
{
	public static class SwaggerConfig
	{
		public static void AddSwaggerConfiguration(this IServiceCollection services)
		{
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen(c =>
			{
				// Informações da API
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "Users.Api",
					Version = "1.0"
				});

				//JWT
				var securityScheme = new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Description = "Insira o token JWT desta forma: {seu token}",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					BearerFormat = "JWT",
					Reference = new OpenApiReference
					{
						Type = ReferenceType.SecurityScheme,
						Id = "Bearer"
					}
				};

				c.AddSecurityDefinition("Bearer", securityScheme);

				var securityRequirement = new OpenApiSecurityRequirement
				{
					{ securityScheme, new[] { "Bearer" } }
				};

				c.AddSecurityRequirement(securityRequirement);
				c.OperationFilter<AuthorizeCheckOperationFilter>();
			});
		}

		public static void UseSwaggerConfiguration(this IApplicationBuilder app)
		{
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
				c.DocumentTitle = "Documentação da API";
				c.RoutePrefix = string.Empty;
			});
		}
	}
}
