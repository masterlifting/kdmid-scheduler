﻿using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;
using KdmidScheduler.Abstractions.Models.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Net.Shared.Abstractions.Models.Settings;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Bots.Abstractions.Models.Response;
using Net.Shared.Extensions.Logging;
using Net.Shared.Extensions.Serialization.Json;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;
using Net.Shared.Persistence.Abstractions.Models.Contexts;

using static KdmidScheduler.Abstractions.Constants;
using static Net.Shared.Bots.Abstractions.Constants;
using static Net.Shared.Persistence.Abstractions.Constants.Enums;

namespace KdmidScheduler.Services;

public sealed class KdmidResponseService(
    ILogger<KdmidResponseService> logger,
    IOptions<CorrelationSettings> correlationOptions,
    IOptions<KdmidSettings> kdmidOptions,
    IBotClient botClient,
    IBotCommandsStore botCommandsStore,
    IKdmidRequestService kdmidRequestService,
    IPersistenceWriterRepository<KdmidAvailableDates> writerRepository
    ) : IKdmidResponseService
{
    private readonly Guid _correlationId = correlationOptions.Value.Id;

    private readonly ILogger<KdmidResponseService> _log = logger;

    private readonly KdmidSettings _kdmidSettings = kdmidOptions.Value;

    private readonly IBotClient _botClient = botClient;
    private readonly IBotCommandsStore _botCommandsStore = botCommandsStore;
    private readonly IKdmidRequestService _kdmidRequestService = kdmidRequestService;
    private readonly IPersistenceWriterRepository<KdmidAvailableDates> _writerRepository = writerRepository;

    public async Task SendAvailableEmbassies(Chat chat, CancellationToken cToken)
    {
        var supportedCities = _kdmidRequestService.GetSupportedCities(cToken);

        var webAppData = new Dictionary<string, Uri>(supportedCities.Length);

        foreach (var city in supportedCities)
        {
            var uri = new Uri($"{_kdmidSettings.WebAppUrl}/kdmidId?chatId={chat.Id}&cityCode={city.Code}");

            webAppData.Add(city.Name, uri);
        }

        if (webAppData.Count == 0)
        {
            await _botClient.SendMessage(new(chat, new("There are no available embassies.")), cToken);
        }
        else
        {
            var webAppArgs = new WebAppEventArgs(chat, new("Available embassies", 3, webAppData));
            await _botClient.SendWebApp(webAppArgs, cToken);
        }
    }
    public async Task SendMyEmbassies(Chat chat, Command command, CancellationToken cToken)
    {
        var supportedCities = _kdmidRequestService.GetSupportedCities(cToken);

        var commands = await _botCommandsStore.Get(chat.Id, cToken);

        var availableCommands = commands
            .Where(x => x.Name == Abstractions.Constants.KdmidBotCommands.CommandInProcess)
            .ToArray();

        var buttonsData = new Dictionary<string, string>(availableCommands.Length);

        foreach (var availableCommand in availableCommands)
        {
            var city = availableCommand.Parameters[BotCommandParametersCityKey].FromJson<City>();
            var kdmidId = availableCommand.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

            var buttonName = $"{city.Name} ({kdmidId.Id})";

            buttonsData.Add(availableCommand.Id.ToString(), buttonName);
        }

        if (buttonsData.Count == 0)
        {
            await _botClient.SendMessage(new(chat, new("You have no embassies in your list.")), cToken);
        }
        else
        {
            var buttonsArgs = new ButtonsEventArgs(chat, new("My embassies", 2, buttonsData));
            await _botClient.SendButtons(buttonsArgs, cToken);
        }
    }

    public async Task SendCreateCommandResult(string chatId, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

        command = await _botCommandsStore.Create(chatId, Abstractions.Constants.KdmidBotCommands.CommandInProcess, new()
        {
            { BotCommandParametersCityKey, command.Parameters[BotCommandParametersCityKey] },
            { BotCommandParametersKdmidIdKey, command.Parameters[BotCommandParametersKdmidIdKey] }
        }, cToken);

        await _writerRepository.CreateOne(new KdmidAvailableDates()
        {
            Chat = new(chatId, new(string.Empty)),
            City = city,
            Command = command,
            StepId = (int)KdmidProcessSteps.CheckAvailableDates,
            StatusId = (int)ProcessStatuses.Ready,
            CorrelationId = _correlationId

        }, cToken);

        await _botClient.SendMessage(chatId, new($"The embassy of '{city.Name}' with Id '{kdmidId.Id}' has been added to processing."), cToken);
        await _botClient.SendMessage(_botClient.AdminId, new($"The embassy of '{city.Name}' with Id '{kdmidId.Id}' has been added to the chat '{chatId}'."), cToken);
    }
    public async Task SendUpdateCommandResult(string chatId, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

        command.Name = Abstractions.Constants.KdmidBotCommands.CommandInProcess;

        await _botCommandsStore.Update(chatId, command.Id, command, cToken);

        var updateOptions = new PersistenceUpdateOptions<KdmidAvailableDates>(x =>
        {
            x.City = city;
            x.Command = command;
            x.StepId = (int)KdmidProcessSteps.CheckAvailableDates;
            x.StatusId = (int)ProcessStatuses.Ready;
        })
        {
            QueryOptions = new()
            {
                Filter = x => x.Chat.Id == chatId && x.Command.Id == command.Id
            }
        };

        await _writerRepository.Update(updateOptions, cToken);

        await _botClient.SendMessage(chatId, new($"The embassy of '{city.Name}' with Id '{kdmidId.Id}' has been updated."), cToken);
    }
    public async Task SendDeleteCommandResult(string chatId, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

        await _botCommandsStore.Delete(chatId, command.Id, cToken);

        var deleteOptions = new PersistenceQueryOptions<KdmidAvailableDates>()
        {
            Filter = x => x.Chat.Id == chatId && x.Command.Id == command.Id
        };

        await _writerRepository.Delete(deleteOptions, cToken);

        await _botClient.SendMessage(chatId, new($"The embassy of '{city.Name}' with Id '{kdmidId.Id}' is deleted."), cToken);
    }

    public Task SendCommandInProcessInfo(Chat chat, Command command, CancellationToken cToken)
    {
        return _botClient.SendMessage(new(chat, new("You have to receive the details of your embassies processes, but it has not been implemented yet.")), cToken);
    }

    public async Task SendAvailableDates(Chat chat, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

        await TryAddAttempt(chat.Id, command, city, kdmidId, cToken);

        var availableDatesResult = await _kdmidRequestService.GetAvailableDates(city, kdmidId, cToken);

        var buttonsData = new Dictionary<string, string>(availableDatesResult.Dates.Count);

        foreach (var date in availableDatesResult.Dates)
        {
            var chosenResult = new ChosenDateResult(availableDatesResult.FormData, date.Key, date.Value);
            var nextCommand = await _botCommandsStore.Create(chat.Id, Abstractions.Constants.KdmidBotCommands.SendConfirmResult, new()
            {
                { BotCommandParametersCityKey, command.Parameters[BotCommandParametersCityKey] },
                { BotCommandParametersKdmidIdKey, command.Parameters[BotCommandParametersKdmidIdKey] },
                { BotCommandParametersChosenResultKey, chosenResult.ToJson() }
            }, cToken);

            buttonsData.Add(nextCommand.Id.ToString(), date.Value);
        }

        if (buttonsData.Count == 0)
        {
            _log.Warn($"Available dates for '{city.Name}' with Id '{kdmidId.Id}' were not found for the chat '{chat.Id}'.");
        }
        else
        {
            await _botClient.SendButtons(new(chat, new($"Available dates for '{city.Name}' with Id '{kdmidId.Id}':", 1, buttonsData)), cToken);
        }
    }
    public async Task SendConfirmationResult(Chat chat, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();
        var chosenResult = command.Parameters[BotCommandParametersChosenResultKey].FromJson<ChosenDateResult>();

        try
        {
            await _kdmidRequestService.ConfirmChosenDate(city, kdmidId, chosenResult, cToken);
            await _botClient.SendMessage(new(chat, new($"'{chosenResult.ChosenValue}' for '{city.Name}' with Id '{kdmidId.Id}' has been confirmed.")), cToken);
        }
        catch
        {
            throw;
        }
        finally
        {
            await _botCommandsStore.Delete(chat.Id, command.Id, cToken);
        }
    }

    public Task SendAskResponse(Chat chat, Command command, CancellationToken cToken)
    {
        Net.Shared.Bots.Abstractions.Models.Response.Message message = command.Parameters.Count == 0
            ? new("To send your message to the developer, use the double quotes.")
            : command.Parameters.TryGetValue(CommandParameters.Message, out var text)
                ? new(text)
                : throw new ArgumentException("The message for the developer is not specified.");

        return _botClient.SendMessage(chat.Id, message, cToken);
    }
    public Task SendAskResponse(string chatId, Command command, CancellationToken cToken)
    {
        Net.Shared.Bots.Abstractions.Models.Response.Message message = command.Parameters.Count == 0
            ? new("To send your message to the developer, use the double quotes.")
            : command.Parameters.TryGetValue(CommandParameters.Message, out var text)
                ? new(text)
                : throw new ArgumentException("The message for the developer is not specified.");

        return _botClient.SendMessage(chatId, message, cToken);
    }
    public Task SendAnswerResponse(Chat chat, Command command, CancellationToken cToken)
    {
        if (!command.Parameters.TryGetValue(CommandParameters.ChatId, out var targetChatId))
            throw new ArgumentException("The chatId for the user is not specified.");

        if (!command.Parameters.TryGetValue(CommandParameters.Message, out var text))
            throw new ArgumentException("The message for the user is not specified.");

        return _botClient.SendMessage(targetChatId, new(text), cToken);
    }

    private async Task TryAddAttempt(string chatId, Command command, City city, KdmidId kdmidId, CancellationToken cToken)
    {
        var attempts = command.Parameters.TryGetValue(BotCommandParametersAttemptsKey, out var attemptsValue)
            ? attemptsValue.FromJson<Attempts>()
            : null;

        var day = DateTime.UtcNow.AddHours(city.TimeShift).DayOfYear;

        if (attempts is null)
        {
            attempts = new Attempts(day, 1);
            command.Parameters.Add(BotCommandParametersAttemptsKey, attempts.ToJson());
        }
        else
        {
            if (attempts.Day == day && attempts.Count >= AttemptsLimit)
                throw new InvalidOperationException($"Attempts limit for '{city.Name}' with Id '{kdmidId.Id}' was reached for the chat '{chatId}'.");

            var attemptsDay = attempts.Day;
            var attemptsCount = (byte)(attempts.Count + 1);

            if (attempts.Day != day)
            {
                attemptsDay = day;
                attemptsCount = 0;
            }

            attempts = new Attempts(attemptsDay, attemptsCount);

            command.Parameters[BotCommandParametersAttemptsKey] = attempts.ToJson();
        }

        await _botCommandsStore.Update(chatId, command.Id, command, cToken);
    }
}
