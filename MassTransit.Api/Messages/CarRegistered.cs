namespace MassTransit.Api.Messages;

public record CarRegistered
{
    public Guid CarId { get; init; }
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public string VIN { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
}
