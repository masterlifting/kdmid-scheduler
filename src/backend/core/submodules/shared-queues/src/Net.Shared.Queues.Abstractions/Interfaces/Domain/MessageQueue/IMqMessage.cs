namespace Net.Shared.Queues.Abstractions.Interfaces.Domain.MessageQueue;

public interface IMqMessage<TPayload> where TPayload : notnull
{
    IMqQueue Queue { get; init; }
    TPayload Payload { get; init; }
    DateTime DateTime { get; init; }
    string? Version { get; init; }
}