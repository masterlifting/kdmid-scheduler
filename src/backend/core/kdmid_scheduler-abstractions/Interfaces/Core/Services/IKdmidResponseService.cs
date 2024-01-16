using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Abstractions.Interfaces.Core.Services;

public interface IKdmidResponseService
{
    Task SendAvailableEmbassies(Chat chat, Command command, CancellationToken cToken);
    Task SendMyEmbassies(Chat chat, Command command, CancellationToken cToken);
    Task SendConfirmationResult(Chat chat, Command command, CancellationToken cToken);
    Task AddAvailableEmbassy(string chatId, Command command, CancellationToken cToken);
    
    Task SendAvailableDates(string chatId, Command command, CancellationToken cToken);
    Task SendAskResponse(string chatId, Command command, CancellationToken cToken);
    Task SendAnswerResponse(string chatId, Command command, CancellationToken cToken);
}
