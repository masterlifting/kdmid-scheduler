using TelegramBot.Abstractions.Models.Telegram;

namespace TelegramBot.Abstractions.Interfaces.Services;

public interface ITelegramBot
{

    Task Start(Message message, CancellationToken cToken);
    Task Help(Message message, CancellationToken cToken);
    Task Stop(Message message, CancellationToken cToken);
}
