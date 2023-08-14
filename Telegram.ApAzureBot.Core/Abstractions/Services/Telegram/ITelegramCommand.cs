using Telegram.ApAzureBot.Core.Persistence.NoSql;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;

public interface ITelegramCommand
{
    Task Execute(TelegramMessage message, CancellationToken cToken);
}
