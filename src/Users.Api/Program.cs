using Microsoft.ApplicationInsights.Extensibility;
using OpenTelemetry.Trace;
using Serilog;
using Users.Api.Configurations;
using Users.Api.Middlewares;
using Users.Application;
using Users.Infra.Data;

var builder = WebApplication.CreateBuilder(args);

// Azure Key Vault via CSI Driver Mount (secrets mounted to /mnt/secrets-store)
// Secrets are synced to K8s secrets automatically via SecretProviderClass
var keyVaultPath = "/mnt/secrets-store";
if (Directory.Exists(keyVaultPath))
{
    Log.Information("Loading secrets from Key Vault CSI Driver mount: {Path}", keyVaultPath);
    LoadSecretsFromKeyVault(builder.Configuration, keyVaultPath);
}

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

// Health Check Endpoints
app.MapGet("/health/live", () => 
{
    Log.Information("Liveness probe called");
    return Results.Ok(new { status = "alive", timestamp = DateTime.UtcNow });
}).WithName("HealthLive").WithOpenApi();

app.MapGet("/health/ready", () =>
{
    try
    {
        // Check if essential services are available
        // In a real scenario, you would check database, redis, elasticsearch connectivity
        Log.Information("Readiness probe called");
        return Results.Ok(new { status = "ready", timestamp = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Readiness probe failed");
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}).WithName("HealthReady").WithOpenApi();

app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "healthy", version = "1.0" });
}).WithName("Health").WithOpenApi();

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

// Helper method to load secrets from Key Vault CSI Driver mount
static void LoadSecretsFromKeyVault(IConfigurationBuilder config, string keyVaultPath)
{
    try
    {
        var secretsDict = new Dictionary<string, string?>();
        
        if (Directory.Exists(keyVaultPath))
        {
            foreach (var file in Directory.GetFiles(keyVaultPath))
            {
                var fileName = Path.GetFileName(file);
                var fileContent = File.ReadAllText(file).Trim();
                
                // Convert filename to config key (e.g., "ConnectionStrings--DefaultConnection" -> "ConnectionStrings:DefaultConnection")
                var configKey = fileName.Replace("--", ":");
                secretsDict[configKey] = fileContent;
                
                Log.Information("Loaded secret from Key Vault: {SecretName}", fileName);
            }
        }
        
        config.AddInMemoryCollection(secretsDict);
        Log.Information("✓ Key Vault secrets loaded successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to load secrets from Key Vault CSI Driver mount");
        // Continue without Key Vault; fall back to appsettings
    }
}

