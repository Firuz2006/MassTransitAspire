using System.Diagnostics;

namespace MassTransit.Api.Filters;

public class CorrelationActivityFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private static readonly ActivitySource ActivitySource = new("MassTransit.Api");

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var correlationIdGuid = context.CorrelationId ?? context.MessageId ?? Guid.NewGuid();
        var traceIdString = correlationIdGuid.ToString("N"); // 32 hex chars without hyphens

        // Convert Guid to ActivityTraceId (128-bit)
        var traceId = ActivityTraceId.CreateFromString(traceIdString.AsSpan());
        var activityContext = new ActivityContext(
            traceId,
            ActivitySpanId.CreateRandom(),
            ActivityTraceFlags.Recorded);

        using var activity = ActivitySource.StartActivity(
            $"Consumer {typeof(T).Name}",
            ActivityKind.Consumer,
            activityContext);

        activity?.SetTag("messaging.message_type", typeof(T).Name);
        activity?.SetTag("messaging.correlation_id", correlationIdGuid);

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("correlationActivityFilter");
    }
}
