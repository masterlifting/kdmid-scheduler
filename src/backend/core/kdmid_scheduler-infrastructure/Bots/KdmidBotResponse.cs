using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using Net.Shared.Bots.Abstractions.Interfaces;

using static Net.Shared.Bots.Abstractions.Constants;
using static KdmidScheduler.Abstractions.Constants;
using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Infrastructure.Bots;

public sealed class KdmidBotResponse(IKdmidResponseService responseService) : IBotResponse
{
    private readonly IKdmidResponseService _responseService = responseService;

    public Task Create(Chat chat, Command command, CancellationToken cToken) => command.Name switch
    {
        Commands.Start => _responseService.SendAvailableEmbassies(chat, command, cToken),
        KdmidBotCommands.Mine => _responseService.SendMyEmbassies(chat, command, cToken),
        KdmidBotCommands.SendConfirmResult => _responseService.SendConfirmationResult(chat, command, cToken),
        _ => throw new NotSupportedException($"The command '{command.Name}' is not supported.")
    };
    public Task Create(string chatId, Command command, CancellationToken cToken) => command.Name switch
    {
        Commands.Ask => _responseService.SendAskResponse(chatId, command, cToken),
        Commands.Answer => _responseService.SendAnswerResponse(chatId, command, cToken),
        KdmidBotCommands.SendAvailableDates => _responseService.SendAvailableDates(chatId, command, cToken),
        KdmidBotCommands.AddAvailableEmbassy => _responseService.AddAvailableEmbassy(chatId, command, cToken),
        _ => throw new NotSupportedException($"The command '{command.Name}' is not supported.")
    };
}
