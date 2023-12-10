using TelegramBot.Abstractions.Models.Telegram;

namespace TelegramBot.Abstractions.Interfaces.Services;

public interface ITelegramCommand
{
    Task Execute(Message message, CancellationToken cToken);
}
