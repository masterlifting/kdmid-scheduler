using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Abstractions.Interfaces.Core.Services;

public interface IKdmidBotApi
{
    Task Listen(CancellationToken cToken);
    Task Listen(Uri uri, CancellationToken cToken);
    Task Receive(StreamReader reader, CancellationToken cToken);
    Task<BotCommand> GetCommand(string chatId, string commandId, CancellationToken cToken);
    Task UpdateCommand(string chatId, StreamReader reader, CancellationToken cToken);
}
