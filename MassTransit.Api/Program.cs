using MassTransit;
using MassTransit.Api.Consumers;
using MassTransit.Api.Filters;
using MassTransit.Api.Messages;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((host, log) =>
{
    if (host.HostingEnvironment.IsProduction())
    {
        log.MinimumLevel.Information();
    }
    else
        log.MinimumLevel.Debug();

    log.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
    log.MinimumLevel.Override("Quartz", LogEventLevel.Information);
    log.Enrich.FromLogContext();
    log.Enrich.WithSpan(); // Enriches logs with Activity TraceId and SpanId for trace grouping
    log.Enrich.WithProperty("Application", "MassTransit.Api");
    // Write to OpenTelemetry instead of Console
    log.WriteTo.OpenTelemetry();
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure OpenTelemetry with distributed tracing and logging
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("MassTransit.Api"))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("MassTransit.Api") // Custom ActivitySource from our filter
            .AddSource("MassTransit") // Built-in MassTransit tracing
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter();
    })
    .WithLogging(logging =>
    {
        logging.AddConsoleExporter();
    });

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

        // Register the correlation activity filter globally
        cfg.UseConsumeFilter(typeof(CorrelationActivityFilter<>), context);

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