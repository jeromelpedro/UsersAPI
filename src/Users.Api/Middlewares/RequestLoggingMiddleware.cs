using System.Diagnostics;

namespace Users.Api.Middlewares
{
	public class RequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<RequestLoggingMiddleware> _logger;

		public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var stopwatch = Stopwatch.StartNew();

			var request = context.Request;
			var traceId = context.TraceIdentifier;
			var correlationId = context.Items[CorrelationIdMiddleware.HeaderKey] as string ?? request.Headers[CorrelationIdMiddleware.HeaderKey].FirstOrDefault() ?? "-";
			_logger.LogInformation("-> {method} {path} | TraceId:{traceId} | CorrelationId:{correlationId} | User:{user}", request.Method, request.Path, traceId, correlationId, context.User?.Identity?.Name ?? "-" );

			await _next(context);

			stopwatch.Stop();

			_logger.LogInformation("<- {statusCode} | Tempo: {elapsed} ms | IP: {ip} | TraceId:{traceId} | CorrelationId:{correlationId}",
				context.Response.StatusCode,
				stopwatch.ElapsedMilliseconds,
				context.Connection.RemoteIpAddress?.ToString(),
				traceId,
				correlationId);
		}
	}

	public static class RequestLoggingMiddlewareExtensions
	{
		public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<RequestLoggingMiddleware>();
		}
	}
}
