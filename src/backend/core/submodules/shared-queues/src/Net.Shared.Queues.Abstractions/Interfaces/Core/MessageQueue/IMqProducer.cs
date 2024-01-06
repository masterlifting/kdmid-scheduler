using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Queues.Abstractions.Interfaces.Domain.MessageQueue;
using Net.Shared.Queues.Abstractions.Models.Settings.MessageQueue;

namespace Net.Shared.Queues.Abstractions.Interfaces.Core.MessageQueue;

public interface IMqProducer : IDisposable
{
    Task Produce<TMessage, TPayload>(IEnumerable<TMessage> messages, MqProducerSettings settings, CancellationToken cToken)
        where TMessage : class, IMqMessage<TPayload>
        where TPayload : notnull;
    Task<Result<bool>> TryProduce<TMessage, TPayload>(IEnumerable<TMessage> messages, MqProducerSettings settings, CancellationToken cToken)
        where TMessage : class, IMqMessage<TPayload>
        where TPayload : notnull;
}
