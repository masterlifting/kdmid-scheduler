using Telegram.ApAzureBot.Core.Persistence.NoSql;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;

public interface ITelegramCommand
{
    Task Process(TelegramMessage message, CancellationToken cToken);
}
