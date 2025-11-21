using MassTransit.Api.Messages;

namespace MassTransit.Api.Consumers;

public class CarMaintenanceScheduledConsumer : IConsumer<CarMaintenanceScheduled>
{
    private readonly ILogger<CarMaintenanceScheduledConsumer> _logger;

    public CarMaintenanceScheduledConsumer(ILogger<CarMaintenanceScheduledConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CarMaintenanceScheduled> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Maintenance scheduled: {MaintenanceId} for Car {CarId} - {ServiceType} on {ScheduledDate}, Cost: ${EstimatedCost}, Description: {Description}",
            message.MaintenanceId,
            message.CarId,
            message.ServiceType,
            message.ScheduledDate,
            message.EstimatedCost,
            message.Description);

        return Task.CompletedTask;
    }
}
