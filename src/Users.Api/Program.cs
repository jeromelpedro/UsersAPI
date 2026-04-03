using Microsoft.ApplicationInsights.Extensibility;
using OpenTelemetry.Trace;
using Serilog;
using Users.Api.Configurations;
using Users.Api.Middlewares;
using Users.Application;
using Users.Infra.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

// pega do container
var telemetryConfiguration = builder.Services.BuildServiceProvider()
	.GetRequiredService<TelemetryConfiguration>();

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.Enrich.FromLogContext()
	.Enrich.With(new Users.Api.Serilog.ActivityEnricher())
	.WriteTo.Console()
	.WriteTo.ApplicationInsights(
		telemetryConfiguration,
		TelemetryConverter.Traces)
	.CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

builder.Services.AddSingleton<RedisConnection>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerConfiguration();
builder.Services.ResolveDependencyInjection(builder.Configuration);
builder.Services.AddAuthConfiguration(builder.Configuration);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// DatabaseUserInitializer.EnsureDatabaseUser(builder.Configuration);
SeedUsuario.Seed(app.Services);

if (app.Environment.IsDevelopment())
	app.MapOpenApi();

app.UseSwagger();
app.UseSwaggerUI();

app.UseErrorHandling();
app.UseCorrelationId();
app.UseRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

