using Net.Shared.Queues.Abstractions.Interfaces.Domain.MessageQueue;

using static Net.Shared.Queues.Abstractions.Constants.Enums.RabbitMq;

namespace Net.Shared.Queues.RabbitMq.Domain;

public sealed class RabbitMqProducerMessage<TPayload> : IMqMessage<TPayload> where TPayload : notnull
{
    public string Id { get; init; } = null!;
    public IMqQueue Queue { get; init; } = null!;
    public TPayload Payload { get; init; } = default!;
    public IDictionary<string, object> Headers { get; init; } = null!;
    public DateTime DateTime { get; init; } = DateTime.UtcNow;

    public ExchangeNames Exchange { get; set; }

    public string? Version { get; init; }
}
