using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

using static Net.Shared.Bots.Abstractions.Constants;
using static KdmidScheduler.Abstractions.Constants;

namespace KdmidScheduler.Infrastructure.Bots;

public sealed class KdmidBotResponse(IKdmidResponseService responseService) : IBotResponse
{
    private readonly IKdmidResponseService _responseService = responseService;

    public Task Create(string chatId, BotCommand command, CancellationToken cToken) => command.Name switch
    {
        Commands.Start => _responseService.SendAvailableEmbassies(chatId, cToken),
        Commands.Ask => _responseService.SendAskResponse(chatId, command, cToken),
        Commands.Answer => _responseService.SendAnswerResponse(chatId, command, cToken),
        KdmidBotCommands.Mine => _responseService.SendMyEmbassies(chatId, cToken),
        KdmidBotCommands.AddAvailableEmbassy => _responseService.AddAvailableEmbassy(chatId, command, cToken),
        KdmidBotCommands.SendAvailableDates => _responseService.SendAvailableDates(chatId, command, cToken),
        KdmidBotCommands.SendConfirmResult => _responseService.SendConfirmationResult(chatId, command, cToken),
        _ => throw new NotSupportedException($"The command '{command.Name}' is not supported.")
    };
}
