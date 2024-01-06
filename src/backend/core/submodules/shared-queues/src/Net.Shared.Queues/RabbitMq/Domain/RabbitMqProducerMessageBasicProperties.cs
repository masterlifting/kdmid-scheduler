using Net.Shared.Queues.Abstractions.Models.Settings.MessageQueue.RabbitMq;

using RabbitMQ.Client;

namespace Net.Shared.Queues.RabbitMq.Domain;

public sealed class RabbitMqProducerMessageBasicProperties<TPayload> : IBasicProperties where TPayload : notnull
{
    private readonly RabbitMqProducerSettings _settings;

    public RabbitMqProducerMessageBasicProperties(RabbitMqProducerSettings settings, RabbitMqProducerMessage<TPayload> message)
    {
        _settings = settings;

        Headers = message.Headers.ToDictionary(x => x.Key, y => y.Value);

        if (message.Version is not null)
            Headers.Add("VERSION", message.Version);
    }

    public ushort ProtocolClassId => 0;
    public string? ProtocolClassName => null;

    public IDictionary<string, object> Headers { get; set; }

    public string? AppId { get; set; }
    public string? ClusterId { get; set; }
    public string? ContentEncoding { get; set; }
    public string? ContentType { get; set; }
    public string? CorrelationId { get; set; }
    public byte DeliveryMode { get; set; }
    public string? Expiration { get; set; }
    public string? MessageId { get; set; }
    public bool Persistent { get; set; }
    public byte Priority { get; set; }
    public string? ReplyTo { get; set; }
    public PublicationAddress? ReplyToAddress { get; set; }
    public AmqpTimestamp Timestamp { get; set; }
    public string? Type { get; set; }
    public string? UserId { get; set; }

    public void ClearAppId() => throw new NotImplementedException();
    public void ClearClusterId() => throw new NotImplementedException();
    public void ClearContentEncoding() => throw new NotImplementedException();
    public void ClearContentType() => throw new NotImplementedException();
    public void ClearCorrelationId() => throw new NotImplementedException();
    public void ClearDeliveryMode() => throw new NotImplementedException();
    public void ClearExpiration() => throw new NotImplementedException();
    public void ClearHeaders() => throw new NotImplementedException();
    public void ClearMessageId() => throw new NotImplementedException();
    public void ClearPriority() => throw new NotImplementedException();
    public void ClearReplyTo() => throw new NotImplementedException();
    public void ClearTimestamp() => throw new NotImplementedException();
    public void ClearType() => throw new NotImplementedException();
    public void ClearUserId() => throw new NotImplementedException();
    public bool IsAppIdPresent() => throw new NotImplementedException();
    public bool IsClusterIdPresent() => throw new NotImplementedException();
    public bool IsContentEncodingPresent() => throw new NotImplementedException();
    public bool IsContentTypePresent() => throw new NotImplementedException();
    public bool IsCorrelationIdPresent() => throw new NotImplementedException();
    public bool IsDeliveryModePresent() => throw new NotImplementedException();
    public bool IsExpirationPresent() => throw new NotImplementedException();
    public bool IsHeadersPresent() => throw new NotImplementedException();
    public bool IsMessageIdPresent() => throw new NotImplementedException();
    public bool IsPriorityPresent() => throw new NotImplementedException();
    public bool IsReplyToPresent() => throw new NotImplementedException();
    public bool IsTimestampPresent() => throw new NotImplementedException();
    public bool IsTypePresent() => throw new NotImplementedException();
    public bool IsUserIdPresent() => throw new NotImplementedException();
}
