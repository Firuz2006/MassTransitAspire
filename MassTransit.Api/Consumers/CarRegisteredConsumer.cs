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

        _logger.LogInformation(
            "Car registered: {CarId} - {Make} {Model} {Year}, VIN: {VIN}, Registered at: {RegisteredAt}",
            message.CarId,
            message.Make,
            message.Model,
            message.Year,
            message.VIN,
            message.RegisteredAt);

        return Task.CompletedTask;
    }
}
