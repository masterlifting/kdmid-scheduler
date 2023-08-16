using Telegram.ApAzureBot.Core.Models;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;

public interface ITelegramCommand
{
    Task Execute(TelegramMessage message, CancellationToken cToken);
}
