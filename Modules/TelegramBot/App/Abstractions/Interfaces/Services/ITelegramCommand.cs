using TelegramBot.Abstractions.Models;

namespace TelegramBot.Abstractions.Interfaces.Services;

public interface ITelegramCommand
{
    Task Execute(TelegramMessage message, CancellationToken cToken);
}
