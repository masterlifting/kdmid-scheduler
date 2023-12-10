using TelegramBot.Abstractions.Interfaces.Services.CommandProcesses;

namespace TelegramBot.Abstractions.Interfaces.Services;

public interface ITelegramServiceProvider
{
    ITelegramClient GetClient();
    T GetService<T>() where T : CommandProcesses.ITelegramCommand;
}
