namespace Net.Shared.Queues.Abstractions.Models.Settings.MessageQueue.RabbitMq;

public sealed record RabbitMqSection
{
    public const string Name = "RabbitMq";
    public RabbitMqClientSettings Client { get; init; } = null!;
    public Dictionary<string, RabbitMqConsumerSettings>? Consumers { get; init; }
}