using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Infrastructure.Bots;

public sealed class KdmidBotResponse(IKdmidResponseService responseService) : IBotResponse
{
    private readonly IKdmidResponseService _responseService = responseService;

    public Task Create(string chatId, BotCommand command, CancellationToken cToken) => command.Name switch
    {
        IKdmidResponseService.StartCommand => _responseService.SendAvailableEmbassies(chatId, cToken),
        IKdmidResponseService.MineCommand => _responseService.SendMyEmbassies(chatId, cToken),
        IKdmidResponseService.SendAvailableDatesCommand => _responseService.SendAvailableDates(chatId, command, cToken),
        IKdmidResponseService.SendConfirmResultCommand => _responseService.SendConfirmationResult(chatId, command, cToken),
        _ => throw new NotSupportedException($"The command '{command}' is not supported.")
    };
}
