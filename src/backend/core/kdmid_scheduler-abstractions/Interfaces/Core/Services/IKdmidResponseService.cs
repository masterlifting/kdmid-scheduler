using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Abstractions.Interfaces.Core.Services;

public interface IKdmidResponseService
{
    Task SendAvailableEmbassies(Message message, CancellationToken cToken);
    Task SendMyEmbassies(Message message, Command command, CancellationToken cToken);

    Task SendCreateCommandResult(Message message, Command command, CancellationToken cToken);
    Task SendUpdateCommandResult(Message message, Command command, CancellationToken cToken);
    Task SendDeleteCommandResult(Message message, Command command, CancellationToken cToken);
    
    Task SendAvailableDates(Message message, Command command, CancellationToken cToken);
    Task SendConfirmationResult(Message message, Command command, CancellationToken cToken);
    Task SendInfo(Message message, Command command, CancellationToken cToken);

    Task SendAskResponse(Message message, Command command, CancellationToken cToken);
    Task SendAnswerResponse(Message message, Command command, CancellationToken cToken);
}
