namespace MassTransit.Api.Messages;

public record CarMaintenanceScheduled
{
    public Guid MaintenanceId { get; init; }
    public Guid CarId { get; init; }
    public string ServiceType { get; init; } = string.Empty;
    public DateTime ScheduledDate { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal EstimatedCost { get; init; }
}
