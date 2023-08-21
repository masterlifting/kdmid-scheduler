namespace Telegram.ApAzureBot.Core.Models;

public sealed record TelegramButtons(long ChatId, string Text, IEnumerable<(string Name, string Callback)> Buttons);
