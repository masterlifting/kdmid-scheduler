using Net.Shared.Queues.Abstractions.Interfaces.Domain.MessageQueue;

namespace Net.Shared.Queues.RabbitMq.Domain;

public sealed record RabbitMqConsumerMessage : IMqMessage<string>
{
    public IMqQueue Queue { get; init; } = null!;
    public string Payload { get; init; } = null!;
    public IDictionary<string, string> Headers { get; init; } = null!;
    public DateTime DateTime { get; init; } = DateTime.UtcNow;

    public string Exchange { get; init; } = null!;
    public string RoutingKey { get; init; } = null!;
    public string ConsumerTag { get; init; } = null!;
    public ulong DeliveryTag { get; init; }

    public string? Version { get; init; }
}