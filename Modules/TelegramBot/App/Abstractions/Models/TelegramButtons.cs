using static TelegramBot.Abstractions.Constants;

namespace TelegramBot.Abstractions.Models;

public sealed record TelegramButtons(long ChatId, string Text, IEnumerable<(string Name, string Callback)> Items, ButtonStyle Style);
