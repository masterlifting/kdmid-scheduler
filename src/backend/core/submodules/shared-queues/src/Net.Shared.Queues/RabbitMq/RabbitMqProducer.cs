using Microsoft.Extensions.Logging;

using Net.Shared.Extensions.Logging;
using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Queues.Abstractions.Interfaces.Core.MessageQueue;
using Net.Shared.Queues.Abstractions.Interfaces.Domain.MessageQueue;
using Net.Shared.Queues.Abstractions.Models.Settings.MessageQueue;
using Net.Shared.Queues.Abstractions.Models.Settings.MessageQueue.RabbitMq;
using Net.Shared.Queues.RabbitMq.Domain;

namespace Net.Shared.Queues.RabbitMq;

public sealed class RabbitMqProducer : IMqProducer
{
    private readonly string _producerInfo;
    private readonly RabbitMqClient _client;
    private readonly ILogger<RabbitMqProducer> _log;

    public RabbitMqProducer(ILogger<RabbitMqProducer> logger, RabbitMqClient client)
    {
        _log = logger;
        _client = client;

        var objectId = GetHashCode();
        _producerInfo = $"RabbitMq producer {objectId}";
    }

    public Task Produce<TMessage, TPayload>(IEnumerable<TMessage> messages, MqProducerSettings settings, CancellationToken cToken)
        where TMessage : class, IMqMessage<TPayload>
        where TPayload : notnull
    {
        var producerSettings =
            settings as RabbitMqProducerSettings
            ?? throw new InvalidOperationException($"Configuration '{nameof(RabbitMqProducerSettings)}' was not found.");

        var _messages = messages is IEnumerable<RabbitMqProducerMessage<TPayload>>
            ? (messages as IEnumerable<RabbitMqProducerMessage<TPayload>>)!
            : throw new InvalidOperationException("Messages have incorrected format.");

        _client.PublishMessagesSync(producerSettings, _messages);

        return Task.CompletedTask;
    }
    public async Task<Result<bool>> TryProduce<TMessage, TPayload>(IEnumerable<TMessage> messages, MqProducerSettings settings, CancellationToken cToken)
        where TMessage : class, IMqMessage<TPayload>
        where TPayload : notnull
    {
        await Produce<TMessage, TPayload>(messages, settings, cToken);
        return new(true);
    }
    public void Dispose()
    {
        _client.Dispose();
        _log.Debug($"{_producerInfo} was disconnected.");
    }
}
