using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Abstractions.Interfaces.Core.Services;

public interface IKdmidBotApi
{
    Task Listen(CancellationToken cToken);
    Task Listen(Uri uri, CancellationToken cToken);
    Task Receive(StreamReader reader, CancellationToken cToken);
    Task<Command> GetCommand(string chatId, string commandId, CancellationToken cToken);
    Task SetCommand(string chatId, StreamReader reader, CancellationToken cToken);
    Task<Command[]> GetCommands(string chatId, Dictionary<string, string> filter, CancellationToken cToken);
}
