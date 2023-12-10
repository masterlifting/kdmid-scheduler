using TelegramBot.Abstractions.Models.Telegram;

namespace TelegramBot.Abstractions.Interfaces.Services;

public interface ITelegramCommand
{
    Task<Command> Map(Message message, CancellationToken cToken);
}
