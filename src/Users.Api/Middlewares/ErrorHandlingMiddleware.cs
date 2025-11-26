using System.Net;
using System.Text.Json;

namespace Users.Api.Middlewares
{
	public class ErrorHandlingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ErrorHandlingMiddleware> _logger;

		public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ocorreu um erro inesperado.");

				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				context.Response.ContentType = "application/json";

				var result = JsonSerializer.Serialize(new
				{
					error = "Ocorreu um erro interno no servidor.",
					details = ex.Message
				});

				await context.Response.WriteAsync(result);
			}
		}
	}

	public static class ErrorHandlingMiddlewareExtensions
	{
		public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<ErrorHandlingMiddleware>();
		}
	}
}
