using System.Text.Json.Serialization;
using WholesaleHub.Api.Infrastructure;
using WholesaleHub.Application;
using WholesaleHub.Infrastructure;
using WholesaleHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "WholesaleHub API"));
}

app.MapControllers();
app.MapHealthChecks("/health");

await PrepareDatabaseAsync(app);
await app.RunAsync();

// Runs migrations + seed data on startup if turned on in appsettings (it is in Development).
// TODO: for prod, run migrations as a deploy step instead (dotnet ef migrations script)
static async Task PrepareDatabaseAsync(WebApplication app)
{
    var options = app.Configuration.GetSection("Database");
    if (!options.GetValue<bool>("MigrateOnStartup"))
        return;

    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (options.GetValue<bool>("SeedSampleData"))
        await DevDataSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<TimeProvider>());
}

// needed so the integration tests can use WebApplicationFactory<Program>
public partial class Program;
