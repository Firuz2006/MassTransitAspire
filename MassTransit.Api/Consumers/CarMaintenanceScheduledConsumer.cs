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

        // Step 1: Log message received
        _logger.LogInformation(
            "[STEP 1/4] Message received - Processing maintenance scheduling for MaintenanceId: {MaintenanceId}",
            message.MaintenanceId);

        // Step 2: Validate scheduled date
        _logger.LogInformation(
            "[STEP 2/4] Validating scheduled date for Car {CarId} - Service Type: {ServiceType}, Date: {ScheduledDate}",
            message.CarId,
            message.ServiceType,
            message.ScheduledDate);

        if (message.ScheduledDate < DateTime.UtcNow)
        {
            _logger.LogWarning(
                "[STEP 2/4] Scheduled date validation warning: Date {ScheduledDate} is in the past",
                message.ScheduledDate);
        }

        // Step 3: Simulate business logic - Notifying service center
        _logger.LogInformation(
            "[STEP 3/4] Notifying service center for {ServiceType} - Estimated Cost: ${EstimatedCost}, Description: {Description}",
            message.ServiceType,
            message.EstimatedCost,
            message.Description);

        // Step 4: Log completion
        _logger.LogInformation(
            "[STEP 4/4] Maintenance scheduling completed successfully - MaintenanceId: {MaintenanceId}, CarId: {CarId}",
            message.MaintenanceId,
            message.CarId);

        return Task.CompletedTask;
    }
}
