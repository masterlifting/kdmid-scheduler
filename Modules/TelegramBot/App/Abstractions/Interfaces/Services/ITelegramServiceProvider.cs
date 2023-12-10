using TelegramBot.Abstractions.Interfaces.Services.CommandProcesses;

namespace TelegramBot.Abstractions.Interfaces.Services;

public interface ITelegramServiceProvider
{
    ITelegramClient GetTelegramClient();
    T GetService<T>() where T : ITelegramCommandProcess;
}
