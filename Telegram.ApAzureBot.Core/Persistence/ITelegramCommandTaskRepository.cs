using Telegram.ApAzureBot.Core.Persistence.NoSql;

namespace Telegram.ApAzureBot.Core.Persistence
{
    public interface ITelegramCommandTaskRepository
    {
        Task CreateCommandTask(TelegramCommandTask task, CancellationToken cToken);
        Task<TelegramCommandTask[]> GetReadyTasks(int limit);
        Task UpdateStatus(IEnumerable<TelegramCommandTask> tasks);
    }
}
