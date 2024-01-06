using Net.Shared.Queues.Abstractions.Models.Domain.MessageQueue.RabbitMq;

namespace Net.Shared.Queues.Abstractions.Models.Settings.MessageQueue.RabbitMq;

public sealed record RabbitMqModelBuilderSettings
{
    public RabbitMqExchange Exchange { get; init; } = null!;
    public RabbitMqQueue Queue { get; init; } = null!;
}