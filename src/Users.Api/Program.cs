using Users.Api.Configurations;
using Users.Api.Middlewares;
using Users.Application;
using Users.Infra.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerConfiguration();
builder.Services.ResolveDependencyInjection(builder.Configuration);
builder.Services.AddAuthConfiguration(builder.Configuration);


var app = builder.Build();

DatabaseUserInitializer.EnsureDatabaseUser(builder.Configuration);
SeedUsuario.Seed(app.Services);

if (app.Environment.IsDevelopment())
	app.MapOpenApi();

app.UseSwagger();
app.UseSwaggerUI();

app.UseErrorHandling();
app.UseRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
