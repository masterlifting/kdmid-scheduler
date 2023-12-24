using TelegramBot.Abstractions.Models.Telegram;

namespace TelegramBot.Abstractions.Interfaces.Services;

public interface ITelegramClient
{
    Task SetWebhook(string url, CancellationToken cToken);
    Task ListenMessages(CancellationToken cToken);
    Task ReceiveMessage(string data, CancellationToken cToken);
    Task SendMessage(Message message, CancellationToken cToken);
    Task SendButtons(Buttons button, CancellationToken cToken);
    Task SendPhoto(Photo photo, CancellationToken cToken);
}
