namespace TelegramBot.Abstractions.Interfaces.Services;

public interface ITelegramServiceProvider
{
    ITelegramClient GetClient();
    T GetService<T>() where T : ITelegramBot;
}
