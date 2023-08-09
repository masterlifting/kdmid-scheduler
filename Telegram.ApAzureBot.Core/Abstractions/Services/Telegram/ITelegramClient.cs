using Telegram.ApAzureBot.Core.Persistence.NoSql;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;

public interface ITelegramClient
{
    Task SetWebhook(string url, CancellationToken cToken);
    Task ListenBot(CancellationToken cToken);
    Task ReceiveMessage(string data, CancellationToken cToken);
    Task SendMessage(TelegramMessage message, CancellationToken cToken);
    Task SendPhoto(TelegramPhoto photo, CancellationToken cToken);
}
