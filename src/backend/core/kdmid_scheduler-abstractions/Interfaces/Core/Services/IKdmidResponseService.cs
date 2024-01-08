using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Abstractions.Interfaces.Core.Services;

public interface IKdmidResponseService
{
    Task SendAvailableEmbassies(string chatId, CancellationToken cToken);
    Task SendMyEmbassies(string chatId, CancellationToken cToken);
    Task SendAvailableDates(string chatId, BotCommand command, CancellationToken cToken);
    Task SendConfirmationResult(string chatId, BotCommand command, CancellationToken cToken);
    Task SendAskResponse(string chatId, BotCommand command, CancellationToken cToken);
    Task SendAnswerResponse(string chatId, BotCommand command, CancellationToken cToken);
}
