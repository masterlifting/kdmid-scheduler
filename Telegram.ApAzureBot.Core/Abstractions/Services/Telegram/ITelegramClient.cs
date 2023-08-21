using Telegram.ApAzureBot.Core.Models;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;

public interface ITelegramClient
{
    Task SetWebhook(string url, CancellationToken cToken);
    Task ListenMessages(CancellationToken cToken);
    Task ReceiveMessage(string data, CancellationToken cToken);
    Task SendMessage(TelegramMessage message, CancellationToken cToken);
    Task SendButtons(TelegramButtons button, CancellationToken cToken);
    Task SendPhoto(TelegramPhoto photo, CancellationToken cToken);
}
