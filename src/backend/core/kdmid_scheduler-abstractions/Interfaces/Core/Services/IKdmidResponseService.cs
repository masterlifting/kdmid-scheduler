using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Abstractions.Interfaces.Core.Services;

public interface IKdmidResponseService
{
    public const string StartCommand = "start";
    public const string MineCommand = "mine";
    public const string SendAvailableDatesCommand = "sendAvailableDates";
    public const string SendConfirmResultCommand = "sendConfirmResult";

    Task SendAvailableEmbassies(string chatId, CancellationToken cToken);
    Task SendMyEmbassies(string chatId, CancellationToken cToken);
    Task SendAvailableDates(string chatId, BotCommand command, CancellationToken cToken);
    Task SendConfirmationResult(string chatId, BotCommand command, CancellationToken cToken);
}
