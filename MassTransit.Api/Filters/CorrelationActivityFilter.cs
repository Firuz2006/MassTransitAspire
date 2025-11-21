using System.Diagnostics;

namespace MassTransit.Api.Filters;

public class CorrelationActivityFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private static readonly ActivitySource ActivitySource = new("MassTransit.Api");

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        using var activity = ActivitySource.StartActivity(
            $"Process {typeof(T).Name}",
            ActivityKind.Consumer);

        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.message_type", typeof(T).Name);
        activity?.SetTag("messaging.correlation_id", context.CorrelationId);
        activity?.SetTag("messaging.message_id", context.MessageId);
        activity?.SetTag("messaging.conversation_id", context.ConversationId);

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("correlationActivityFilter");
    }
}