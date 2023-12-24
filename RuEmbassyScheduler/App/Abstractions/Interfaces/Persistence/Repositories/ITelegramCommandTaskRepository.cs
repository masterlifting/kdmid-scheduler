using TelegramBot.Abstractions.Models;
using TelegramBot.Abstractions.Models.Persistence.Entities;

namespace TelegramBot.Abstractions.Interfaces.Persistence.Repositories;

public interface ITelegramCommandTaskRepository
{
    Task StartTask(TelegramMessage message, CancellationToken cToken);
    Task<TelegramCommandTask[]> GetReadyTasks(string[] cities, CancellationToken cToken);
    Task UpdateTaskStatus(IEnumerable<TelegramCommandTask> tasks);
    Task StopTask(TelegramMessage message, CancellationToken cToken);
    Task<bool> IsExists(TelegramMessage message, CancellationToken cToken);
}
