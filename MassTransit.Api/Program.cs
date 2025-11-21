using MassTransit;
using MassTransit.Api.Consumers;
using MassTransit.Api.Messages;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    // Register consumers
    x.AddConsumer<CarRegisteredConsumer>();
    x.AddConsumer<CarMaintenanceScheduledConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("messaging");
        cfg.Host(new Uri(connectionString!));
        cfg.ConfigureEndpoints(context);
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}



var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Car registration endpoint
app.MapPost("/cars/register", async (CarRegistered carRegistration, IPublishEndpoint publishEndpoint) =>
{
    await publishEndpoint.Publish(carRegistration);
    return Results.Ok(new { message = "Car registration published", carId = carRegistration.CarId });
})
.WithName("RegisterCar");

// Car maintenance scheduling endpoint
app.MapPost("/cars/maintenance", async (CarMaintenanceScheduled maintenance, IPublishEndpoint publishEndpoint) =>
{
    await publishEndpoint.Publish(maintenance);
    return Results.Ok(new { message = "Maintenance scheduled", maintenanceId = maintenance.MaintenanceId });
})
.WithName("ScheduleCarMaintenance");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
