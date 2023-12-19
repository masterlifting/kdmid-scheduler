using static Net.Shared.Bots.Abstractions.Constants;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotMessage
{
    long ChatId { get; init; }
    string Data { get; init; }
    BotMessageType Type { get; init; }
}
