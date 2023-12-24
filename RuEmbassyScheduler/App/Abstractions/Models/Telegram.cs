using static TelegramBot.Abstractions.Constants;

namespace TelegramBot.Abstractions.Models.Telegram;

public sealed record Command(long ChatId, string Name, string? Parameters);
public sealed record Message(long ChatId, string Text);
public sealed record Buttons(long ChatId, string Text, IEnumerable<(string Name, string Callback)> Items, ButtonStyle Style);
public sealed record Photo(long ChatId, byte[] Payload, string Name, string? Description = null);

