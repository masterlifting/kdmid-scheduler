using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;

namespace Telegram.ApAzureBot.Core.Abstractions.Services;

public interface ITelegramServiceProvider
{
    ITelegramClient GetTelegramClient();
    T GetService<T>() where T : ITelegramCommandProcess;
}
