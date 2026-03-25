using ApiService.Data;
using ApiService.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext with PostgreSQL via Aspire service discovery (if available)
// In test environments, this will be overridden by the test configuration
try
{
    if (builder.Environment.EnvironmentName != "Testing")
    {
        builder.AddNpgsqlDbContext<ApplicationDbContext>("triagedb");
    }
    else
    {
        // Fallback to InMemory for testing
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("TriageDashboard"));
    }
}
catch (InvalidOperationException)
{
    // Fallback to InMemory if Aspire services not available (e.g., in tests)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("TriageDashboard"));
}

var app = builder.Build();

// Apply migrations and seed data (skip for in-memory databases used in tests)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Only run migrations and seeding if using a relational database provider
    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// Register failure endpoints
app.MapFailureEndpoints();
app.MapFailureActionEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Make Program accessible for testing
public partial class Program { }
