using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Repositories;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Models.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Net.Shared.Abstractions.Models.Exceptions;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Bots.Abstractions.Models.Response;
using Net.Shared.Extensions.Logging;
using Net.Shared.Extensions.Serialization.Json;

using static KdmidScheduler.Abstractions.Constants;
using static Net.Shared.Bots.Abstractions.Constants;

namespace KdmidScheduler.Services;

public sealed class KdmidResponseService(
    ILogger<KdmidResponseService> logger,
    IOptions<KdmidSettings> kdmidOptions,
    IBotClient botClient,
    IBotCommandsStore botCommandsStore,
    IKdmidRequestService kdmidRequestService,
    IKdmidResponseRepository repository
    ) : IKdmidResponseService
{

    private readonly ILogger<KdmidResponseService> _log = logger;

    private readonly KdmidSettings _kdmidSettings = kdmidOptions.Value;

    private readonly IBotClient _botClient = botClient;
    private readonly IBotCommandsStore _botCommandsStore = botCommandsStore;
    private readonly IKdmidRequestService _kdmidRequestService = kdmidRequestService;
    private readonly IKdmidResponseRepository _repository = repository;

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
            .Where(x => x.Name == KdmidBotCommandNames.CommandInProcess)
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

        await _repository.Create(KdmidBotCommandNames.CommandInProcess, chatId, city, kdmidId, cToken);

        await _botClient.SendMessage(chatId, new($"The embassy of '{city.Name}' with Id '{kdmidId.Id}' has been added to processing."), cToken);
        await _botClient.SendMessage(_botClient.AdminId, new($"The embassy of '{city.Name}' with Id '{kdmidId.Id}' has been added to the chat '{chatId}'."), cToken);
    }
    public async Task SendUpdateCommandResult(string chatId, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

        command.Name = KdmidBotCommandNames.CommandInProcess;

        await _repository.Update(command, chatId, city, kdmidId, cToken);

        await _botClient.SendMessage(chatId, new($"The embassy of '{city.Name}' with Id '{kdmidId.Id}' has been updated."), cToken);
    }
    public async Task SendDeleteCommandResult(string chatId, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

        await _repository.Clear(chatId, command.Id, cToken);

        await _botClient.SendMessage(chatId, new($"The embassy of '{city.Name}' with Id '{kdmidId.Id}' has been deleted."), cToken);
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
            var nextCommand = await _botCommandsStore.Create(chat.Id, KdmidBotCommandNames.SendConfirmResult, new()
            {
                { BotCommandParametersCityKey, command.Parameters[BotCommandParametersCityKey] },
                { BotCommandParametersKdmidIdKey, command.Parameters[BotCommandParametersKdmidIdKey] },
                { BotCommandParametersChosenResultKey, chosenResult.ToJson() }
            }, cToken);

            buttonsData.Add(nextCommand.Id.ToString(), date.Key);
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
            await _botClient.SendMessage(new(chat, new($"'{chosenResult.ChosenKey}' for '{city.Name}' with Id '{kdmidId.Id}' has been confirmed.")), cToken);
            await _repository.Clear(chat.Id, command.Id, cToken);
        }
        catch (UserInvalidOperationException exception)
        {
            throw new UserInvalidOperationException($"The confirmation of '{chosenResult.ChosenKey}' for '{city.Name}' with Id '{kdmidId.Id}' was failed for the chat '{chat.Id}'. Reason: {exception.Message}");
        }
        catch
        {
            throw;
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
