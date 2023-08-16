using Telegram.ApAzureBot.Core.Models;
using Telegram.ApAzureBot.Core.Persistence.Entities;

namespace Telegram.ApAzureBot.Core.Abstractions.Persistence.Repositories;

public interface ITelegramCommandTaskRepository
{
    Task CreateTask(TelegramMessage message, CancellationToken cToken);
    Task<TelegramCommandTask[]> GetReadyTasks(int limit);
    Task UpdateTaskStatus(IEnumerable<TelegramCommandTask> tasks);
}
