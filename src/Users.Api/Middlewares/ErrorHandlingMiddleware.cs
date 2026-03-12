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
				var traceId = context.TraceIdentifier;
				var correlationId = context.Items[CorrelationIdMiddleware.HeaderKey] as string ?? context.Request.Headers[CorrelationIdMiddleware.HeaderKey].FirstOrDefault();
				_logger.LogError(ex, "Unhandled exception for {path} | TraceId:{traceId} | CorrelationId:{correlationId}", context.Request?.Path.Value, traceId, correlationId);

				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				context.Response.ContentType = "application/json";

				var result = JsonSerializer.Serialize(new
				{
					error = "Ocorreu um erro interno no servidor.",
					details = ex.Message,
					traceId,
					correlationId
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
