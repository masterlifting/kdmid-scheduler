using Net.Shared.Queues.Abstractions.Models.Settings.MessageQueue.Connections;

namespace Net.Shared.Queues.Abstractions.Models.Settings.MessageQueue.RabbitMq;

public sealed record RabbitMqConnectionSettings : NetSharedQueuesConnectionSettings
{
    public override string ConnectionString => $"amqp://{User}:{Password}@{Host}:{Port}";
}
