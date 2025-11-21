using MassTransit.Api.Messages;

namespace MassTransit.Api.Consumers;

public class CarRegisteredConsumer : IConsumer<CarRegistered>
{
    private readonly ILogger<CarRegisteredConsumer> _logger;

    public CarRegisteredConsumer(ILogger<CarRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CarRegistered> context)
    {
        var message = context.Message;

        // Step 1: Log message received
        _logger.LogInformation(
            "[STEP 1/4] Message received - Processing car registration for CarId: {CarId}",
            message.CarId);

        // Step 2: Validate VIN format
        _logger.LogInformation(
            "[STEP 2/4] Validating VIN format for {Make} {Model} - VIN: {VIN}",
            message.Make,
            message.Model,
            message.VIN);

        if (message.VIN.Length != 17)
        {
            _logger.LogWarning(
                "[STEP 2/4] VIN validation warning: VIN length is {Length}, expected 17 characters",
                message.VIN.Length);
        }

        // Step 3: Simulate business logic - Adding to fleet database
        _logger.LogInformation(
            "[STEP 3/4] Adding car to fleet database - {Make} {Model} {Year}, Registered: {RegisteredAt}",
            message.Make,
            message.Model,
            message.Year,
            message.RegisteredAt);

        // Step 4: Log completion
        _logger.LogInformation(
            "[STEP 4/4] Car registration completed successfully - CarId: {CarId}",
            message.CarId);

        return Task.CompletedTask;
    }
}
