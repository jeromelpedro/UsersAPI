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
			_logger.LogInformation("-> {method} {path}", request.Method, request.Path);

			await _next(context);

			stopwatch.Stop();

			_logger.LogInformation("<- {statusCode} | Tempo: {elapsed} ms | IP: {ip}",
				context.Response.StatusCode,
				stopwatch.ElapsedMilliseconds,
				context.Connection.RemoteIpAddress?.ToString());
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
