using KdmidScheduler.Abstractions.Models.Core.v1.KdmidApi;

namespace KdmidScheduler.Abstractions.Interfaces.Core.Services;

public interface IKdmidBotApi
{
    Task Listen(CancellationToken cToken);
    Task Listen(Uri uri, CancellationToken cToken);
    Task Receive(StreamReader reader, CancellationToken cToken);
    Task<CommandGetDto> GetCommand(string chatId, string commandId, CancellationToken cToken);
    Task<CommandGetDto[]> GetCommands(string chatId, string? names, string? cityCode, CancellationToken cToken);
    Task<string> CreateCommand(string chatId, CommandSetDto command, CancellationToken cToken);
    Task UpdateCommand(string chatId, string commandId, CommandSetDto command, CancellationToken cToken);
    Task DeleteCommand(string chatId, string commandId, CancellationToken cToken);
    Task<CityGetDto[]> GetCities(CancellationToken cToken);
}
