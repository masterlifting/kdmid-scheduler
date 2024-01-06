using static Net.Shared.Queues.Abstractions.Constants.Enums.RabbitMq;

namespace Net.Shared.Queues.Abstractions.Models.Domain.MessageQueue.RabbitMq;

public sealed record RabbitMqExchange
{
    public ExchangeTypes Type { get; init; }
    public ExchangeNames Name { get; init; }
    public bool IsDurable { get; init; }
    public bool IsAutoDelete { get; init; }
    public IDictionary<string, object>? Arguments { get; init; }
}
