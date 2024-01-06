using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Net.Shared.Extensions.Logging;
using Net.Shared.Queues.Abstractions.Models.Domain.MessageQueue.RabbitMq;
using Net.Shared.Queues.Abstractions.Models.Settings.MessageQueue.RabbitMq;
using Net.Shared.Queues.RabbitMq.Domain;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Net.Shared.Queues.RabbitMq;

public sealed class RabbitMqClient : IDisposable
{
    private bool _isConsumerReady;
    private readonly string _clientInfo;

    private IModel? _model;
    private readonly ILogger<RabbitMqClient> _logger;
    private readonly RabbitMqClientSettings _clientSettings;

    public RabbitMqClient(ILogger<RabbitMqClient> logger, IOptions<RabbitMqClientSettings> options)
    {
        _logger = logger;
        _clientSettings = options.Value;

        var objectId = GetHashCode();
        _clientInfo = $"RabbitMq client {objectId}";
    }

    public void RegisterConsumerSync(RabbitMqConsumerSettings consumerSettings, AsyncEventHandler<BasicDeliverEventArgs> receivedHandler)
    {
        if (_isConsumerReady)
            return;

        ConnectSync();

        _logger.Trace($"{_clientInfo} registering...");

        foreach (var item in _clientSettings.ModelBuilders)
            RegisterModelSync(item.Exchange, item.Queue);

        _logger.Trace($"{_clientInfo} has registered.");

        _logger.Trace($"{_clientInfo} subscribing...");
        var consumer = new AsyncEventingBasicConsumer(_model);

        consumer.Received += receivedHandler;

        _model!.BasicConsume(
            consumerSettings.Queue,
            consumerSettings.IsAutoAck,
            consumerSettings.ConsumerTag,
            consumerSettings.IsNoLocal,
            consumerSettings.IsExclusiveQueue,
            consumerSettings.Arguments,
            consumer);

        _logger.Trace($"{_clientInfo} has subscribed");

        _isConsumerReady = true;
    }

    public void PublishMessageSync<TPayload>(RabbitMqProducerSettings producerSettings, RabbitMqProducerMessage<TPayload> message)
        where TPayload : notnull
    {
        ConnectSync();

        _logger.Trace($"{_clientInfo} publishing message with Id '{message.Id}'...");

        var exchangeName = string.Intern($"{message.Exchange}");

        _model.BasicPublish(
            exchangeName
            , $"{exchangeName}.{message.Queue.Name}"
            , new RabbitMqProducerMessageBasicProperties<TPayload>(producerSettings, message)
            , Encoding.UTF8.GetBytes("message.Payload.SerializeSync()"));

        _logger.Trace($"{_clientInfo} the message with Id '{message.Id}' was published.");
    }
    public void PublishMessagesSync<TPayload>(RabbitMqProducerSettings producerSettings, IEnumerable<RabbitMqProducerMessage<TPayload>> messages)
        where TPayload : notnull
    {
        ConnectSync();

        _logger.Trace($"{_clientInfo} publishing messages with count '{messages.Count()}'...");
        _ = typeof(TPayload) is not null;

        foreach (var message in messages)
        {
            var exchangeName = string.Intern($"{message.Exchange}");

            _model.BasicPublish(
                exchangeName
                , $"{exchangeName}.{message.Queue.Name}"
                , new RabbitMqProducerMessageBasicProperties<TPayload>(producerSettings, message)
                , Encoding.UTF8.GetBytes("message.Payload.SerializeSync()"));
        }

        _logger.Trace($"{_clientInfo} the message with count '{messages.Count()}' was published.");
    }

    public void Dispose() => _model?.Dispose();

    private void ConnectSync()
    {
        if (_model is null)
        {
            _logger.Trace($"{_clientInfo} connecting...");

            var connectionFactory = new ConnectionFactory
            {
                HostName = _clientSettings.Connection.Host,
                UserName = _clientSettings.Connection.User,
                Password = _clientSettings.Connection.Password
            };

            using var connection = connectionFactory.CreateConnection();
            _model = connection.CreateModel();

            _logger.Trace($"{_clientInfo} has connected.");
        }
    }
    private void RegisterModelSync(RabbitMqExchange exchange, RabbitMqQueue queue)
    {
        var exchangeType = string.Intern(exchange.Type.ToString());
        var exchangeName = string.Intern(exchange.Name.ToString());
        var routingKey = string.Intern($"{exchangeName}.{queue.Name}.*");

        _model!.ExchangeDeclare(exchangeName, exchangeType, exchange.IsDurable, exchange.IsAutoDelete, exchange.Arguments);
        _model.QueueDeclare(queue.Name, queue.IsDurable, queue.IsExclusive, queue.IsAutoDelete, queue.Arguments);

        _model.QueueBind(queue.Name, exchangeName, routingKey);
    }
}
