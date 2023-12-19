using Net.Shared.Bots.Abstractions.Interfaces;

using static Net.Shared.Bots.Abstractions.Constants;

namespace Net.Shared.Bots.Abstractions.Models;

public sealed record BotMessage : IBotMessage
{
    public long ChatId { get; init; }
    public string Data { get; init; } = null!;
    public BotMessageType Type { get; init; }
}

