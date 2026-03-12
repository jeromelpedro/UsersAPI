using Serilog;
using OpenTelemetry.Trace;
using Users.Api.Configurations;
using Users.Api.Middlewares;
using Users.Application;
using Users.Infra.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger, dispose: true);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerConfiguration();
builder.Services.ResolveDependencyInjection(builder.Configuration);
builder.Services.AddAuthConfiguration(builder.Configuration);
builder.Services.AddApplicationInsightsTelemetry();

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.Enrich.FromLogContext()
	.Enrich.With(new Users.Api.Serilog.ActivityEnricher())
	.WriteTo.Console()
	.WriteTo.ApplicationInsights(
		Environment.GetEnvironmentVariable("ApplicationInsights__ConnectionString"),
		TelemetryConverter.Traces)
	.CreateLogger();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

//DatabaseUserInitializer.EnsureDatabaseUser(builder.Configuration);
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

