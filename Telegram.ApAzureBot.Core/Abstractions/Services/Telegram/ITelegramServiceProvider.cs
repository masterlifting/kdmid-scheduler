namespace Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;

public interface ITelegramServiceProvider
{
    ITelegramClient GetTelegramClient();
    T GetService<T>() where T : ITelegramCommandProcess;
}
