using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using Net.Shared.Bots.Abstractions.Interfaces;

using static Net.Shared.Bots.Abstractions.Constants;
using static KdmidScheduler.Abstractions.Constants;
using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Infrastructure.Bots;

public sealed class KdmidBotResponse(IKdmidResponseService responseService) : IBotResponse
{
    private readonly IKdmidResponseService _responseService = responseService;

    public Task Create(Message message, Command command, CancellationToken cToken) => command.Name switch
    {
        Commands.Start => _responseService.SendAvailableEmbassies(message, command, cToken),
        KdmidBotCommandNames.Mine => _responseService.SendMyEmbassies(message, command, cToken),

        KdmidBotCommandNames.CreateCommand => _responseService.SendCreateCommandResult(message, command, cToken),
        KdmidBotCommandNames.UpdateCommand => _responseService.SendUpdateCommandResult(message, command, cToken),
        KdmidBotCommandNames.DeleteCommand => _responseService.SendDeleteCommandResult(message, command, cToken),
        
        KdmidBotCommandNames.SendAvailableDates => _responseService.SendAvailableDates(message, command, cToken),
        KdmidBotCommandNames.SendConfirmResult => _responseService.SendConfirmationResult(message, command, cToken),
        KdmidBotCommandNames.CommandInProcess => _responseService.SendInfo(message, command, cToken),

        Commands.Ask => _responseService.SendAskResponse(message, command, cToken),
        Commands.Answer => _responseService.SendAnswerResponse(message, command, cToken),
        
        _ => throw new NotSupportedException($"The command '{command.Name}' is not supported.")
    };
}
