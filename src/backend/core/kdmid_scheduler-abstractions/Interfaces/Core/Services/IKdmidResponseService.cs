using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Abstractions.Interfaces.Core.Services;

public interface IKdmidResponseService
{
    Task SendAvailableEmbassies(Chat chat, CancellationToken cToken);
    Task SendMyEmbassies(Chat chat, Command command, CancellationToken cToken);
    Task SendCreateCommandResult(string chatId, Command command, CancellationToken cToken);
    Task SendUpdateCommandResult(string chatId, Command command, CancellationToken cToken);
    Task SendDeleteCommandResult(string chatId, Command command, CancellationToken cToken);
    
    Task SendCommandInProcessInfo(Chat chat, Command command, CancellationToken cToken);
    Task SendConfirmationResult(Chat chat, Command command, CancellationToken cToken);
    Task SendAvailableDates(Chat chatId, Command command, CancellationToken cToken);

    Task SendAskResponse(Chat chat, Command command, CancellationToken cToken);
    Task SendAskResponse(string chatId, Command command, CancellationToken cToken);
    Task SendAnswerResponse(Chat chat, Command command, CancellationToken cToken);
}
