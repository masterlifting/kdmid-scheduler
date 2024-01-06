using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Queues.Abstractions.Interfaces.Domain.MessageQueue;
using Net.Shared.Queues.Abstractions.Models.Settings.MessageQueue;

namespace Net.Shared.Queues.Abstractions.Interfaces.Core.MessageQueue;

public interface IMqConsumer : IDisposable
{
    Task Consume<TMessage, TPayload>(
        Func<MqConsumerSettings, IEnumerable<TMessage>, CancellationToken, Task> handler,
        MqConsumerSettings settings,
        CancellationToken cToken)
        where TMessage : class, IMqMessage<TPayload>
        where TPayload : notnull;
    Task<Result<bool>> TryConsume<TMessage, TPayload>(
        Func<MqConsumerSettings, IEnumerable<TMessage>, CancellationToken, Task> handler,
        MqConsumerSettings settings,
        CancellationToken cToken)
        where TMessage : class, IMqMessage<TPayload>
        where TPayload : notnull;
}
