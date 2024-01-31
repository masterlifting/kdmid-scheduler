﻿using KdmidScheduler.Abstractions.Interfaces.Core.Services;
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
        KdmidBotCommandNames.Mine => _responseService.SendMyEmbassies(chat, command, cToken),
        
        KdmidBotCommandNames.CommandInProcess => _responseService.SendCommandInProcessInfo(chat, command, cToken),

        Commands.Ask => _responseService.SendAskResponse(chat, command, cToken),
        Commands.Answer => _responseService.SendAnswerResponse(chat, command, cToken),
        
        KdmidBotCommandNames.SendConfirmResult => _responseService.SendConfirmationResult(chat, command, cToken),
        KdmidBotCommandNames.SendAvailableDates => _responseService.SendAvailableDates(chat, command, cToken),
        _ => throw new NotSupportedException($"The command '{command.Name}' is not supported.")
    };
    public Task Create(string chatId, Command command, CancellationToken cToken) => command.Name switch
    {
        Commands.Ask => _responseService.SendAskResponse(chatId, command, cToken),
        KdmidBotCommandNames.CreateCommand => _responseService.SendCreateCommandResult(chatId, command, cToken),
        KdmidBotCommandNames.UpdateCommand => _responseService.SendUpdateCommandResult(chatId, command, cToken),
        KdmidBotCommandNames.DeleteCommand => _responseService.SendDeleteCommandResult(chatId, command, cToken),
        _ => throw new NotSupportedException($"The command '{command.Name}' is not supported.")
    };
}
