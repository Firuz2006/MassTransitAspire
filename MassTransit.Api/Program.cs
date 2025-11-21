using Azure.Monitor.OpenTelemetry.Exporter;
using MassTransit;
using MassTransit.Api.Consumers;
using MassTransit.Api.Filters;
using MassTransit.Api.Messages;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((host, log) =>
{
    if (host.HostingEnvironment.IsProduction())
        log.MinimumLevel.Information();
    else
        log.MinimumLevel.Debug();

    log.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
    log.MinimumLevel.Override("Quartz", LogEventLevel.Information);
    log.Enrich.FromLogContext();
    log.Enrich.WithSpan();
    log.Enrich.WithProperty("Application", "MassTransit.Api");
    log.WriteTo.OpenTelemetry();
    // log.WriteTo.Console();
});

builder.Services.AddOpenApi();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("MassTransit.Api"))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("MassTransit.Api")
            .AddSource("MassTransit")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddAzureMonitorMetricExporter(options =>
            {
                options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            });
    });

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumer<CarRegisteredConsumer>();
    x.AddConsumer<CarMaintenanceScheduledConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("messaging");
        cfg.Host(new Uri(connectionString!));

        cfg.UseConsumeFilter(typeof(CorrelationActivityFilter<>), context);

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

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

app.MapPost("/cars/register",
        async (CarRegistered carRegistration, IPublishEndpoint publishEndpoint, ILogger<Program> logger) =>
        {
            logger.LogInformation("Publishing car registration for CarId: {CarId}", carRegistration.CarId);
            await publishEndpoint.Publish(carRegistration);
            logger.LogInformation("Car registration published successfully");
            return Results.Ok(new { message = "Car registration published", carId = carRegistration.CarId });
        })
    .WithName("RegisterCar");

app.MapPost("/cars/maintenance",
        async (CarMaintenanceScheduled maintenance, IPublishEndpoint publishEndpoint, ILogger<Program> logger) =>
        {
            logger.LogInformation("Publishing maintenance schedule for MaintenanceId: {MaintenanceId}",
                maintenance.MaintenanceId);
            await publishEndpoint.Publish(maintenance);
            logger.LogInformation("Maintenance scheduled successfully");
            return Results.Ok(new { message = "Maintenance scheduled", maintenanceId = maintenance.MaintenanceId });
        })
    .WithName("ScheduleCarMaintenance");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}