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
        Commands.Start => _responseService.SendAvailableEmbassies(chat, cToken),
        KdmidBotCommands.Mine => _responseService.SendMyEmbassies(chat, command, cToken),
        
        KdmidBotCommands.CommandInProcess => _responseService.SendCommandInProcessInfo(chat, command, cToken),

        Commands.Ask => _responseService.SendAskResponse(chat, command, cToken),
        Commands.Answer => _responseService.SendAnswerResponse(chat, command, cToken),
        
        KdmidBotCommands.SendConfirmResult => _responseService.SendConfirmationResult(chat, command, cToken),
        KdmidBotCommands.SendAvailableDates => _responseService.SendAvailableDates(chat, command, cToken),
        _ => throw new NotSupportedException($"The command '{command.Name}' is not supported.")
    };
    public Task Create(string chatId, Command command, CancellationToken cToken) => command.Name switch
    {
        Commands.Ask => _responseService.SendAskResponse(chatId, command, cToken),
        KdmidBotCommands.CreateCommand => _responseService.SendCreateCommandResult(chatId, command, cToken),
        KdmidBotCommands.UpdateCommand => _responseService.SendUpdateCommandResult(chatId, command, cToken),
        KdmidBotCommands.DeleteCommand => _responseService.SendDeleteCommandResult(chatId, command, cToken),
        _ => throw new NotSupportedException($"The command '{command.Name}' is not supported.")
    };
}
