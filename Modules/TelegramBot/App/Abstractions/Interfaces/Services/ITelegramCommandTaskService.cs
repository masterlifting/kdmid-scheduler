namespace TelegramBot.Abstractions.Interfaces.Services;

public interface ITelegramCommandTaskService
{
    Task Process(string[] cities);
}
