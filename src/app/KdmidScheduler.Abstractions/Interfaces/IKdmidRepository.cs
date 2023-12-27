//using Net.Shared.Bots.Abstractions.Interfaces;
//using TelegramBot.Abstractions.Models.Persistence.Entities;

//namespace KdmidScheduler.Abstractions.Interfaces;

//public interface IKdmidRepository
//{
//    Task StartTask(IBotMessage message, CancellationToken cToken);
//    Task<TelegramCommandTask[]> GetReadyTasks(string[] cities, CancellationToken cToken);
//    Task UpdateTaskStatus(IEnumerable<TelegramCommandTask> tasks);
//    Task StopTask(IBotMessage message, CancellationToken cToken);
//    Task<bool> IsExists(IBotMessage message, CancellationToken cToken);
//}
