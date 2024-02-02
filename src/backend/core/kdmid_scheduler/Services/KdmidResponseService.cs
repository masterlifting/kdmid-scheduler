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

    public async Task SendAvailableEmbassies(Message message, CancellationToken cToken)
    {
        var supportedCities = _kdmidRequestService.GetSupportedCities(cToken);

        var webAppData = new Dictionary<string, Uri>(supportedCities.Length);

        foreach (var city in supportedCities)
        {
            var uri = new Uri($"{_kdmidSettings.WebAppUrl}/kdmidId?chatId={message.Id}&cityCode={city.Code}");

            webAppData.Add(city.Name, uri);
        }

        if (webAppData.Count == 0)
        {
            await _botClient.SendText(new(message, new("There are no available embassies.")), cToken);
        }
        else
        {
            var webAppArgs = new WebAppEventArgs(message, new("Available embassies.", webAppData));
            await _botClient.SendWebApp(webAppArgs, cToken);
        }
    }
    public async Task SendMyEmbassies(Message message, Command command, CancellationToken cToken)
    {
        var commands = await _botCommandsStore.Get(message.Chat.Id, cToken);

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
            await _botClient.SendText(new(message, new("You have no embassies in your list.")), cToken);
        }
        else
        {
            var buttonsArgs = new ButtonsEventArgs(message, new("My embassies.", buttonsData));
            await _botClient.SendButtons(buttonsArgs, cToken);
        }
    }

    public async Task SendCreateCommandResult(Message message, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

        await _repository.Create(KdmidBotCommandNames.CommandInProcess, message.Chat.Id, city, kdmidId, cToken);

        await _botClient.SendText(new(message, new($"The embassy of '{city.Name}' with Id '{kdmidId.Id}' has been added to processing.")), cToken);
        
        var adminMessage = new Message(null, new(_botClient.AdminId));
        await _botClient.SendText(new(adminMessage, new($"The embassy of '{city.Name}' with Id '{kdmidId.Id}' has been added to the chat '{message}'.")), cToken);
    }
    public async Task SendUpdateCommandResult(Message message, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

        command.Name = KdmidBotCommandNames.CommandInProcess;

        await _repository.Update(command, message.Chat.Id, city, kdmidId, cToken);

        await _botClient.SendText(new(message, new($"The embassy of '{city.Name}' with Id '{kdmidId.Id}' has been updated.")), cToken);
    }
    public async Task SendDeleteCommandResult(Message message, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

        await _repository.Clear(message.Chat.Id, command.Id, cToken);

        await _botClient.SendText(new(message, new($"The embassy of '{city.Name}' with Id '{kdmidId.Id}' has been deleted.")), cToken);
    }

    public async Task SendAvailableDates(Message message, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

        await TryAddAttempt(message.Chat.Id, command, city, kdmidId, cToken);

        var availableDatesResult = await _kdmidRequestService.GetAvailableDates(city, kdmidId, cToken);

        var buttonsData = new Dictionary<string, string>(availableDatesResult.Dates.Count);

        foreach (var date in availableDatesResult.Dates)
        {
            var chosenResult = new ChosenDateResult(availableDatesResult.FormData, date.Key, date.Value);
            var nextCommand = await _botCommandsStore.Create(message.Chat.Id, KdmidBotCommandNames.SendConfirmResult, new()
            {
                { BotCommandParametersCityKey, command.Parameters[BotCommandParametersCityKey] },
                { BotCommandParametersKdmidIdKey, command.Parameters[BotCommandParametersKdmidIdKey] },
                { BotCommandParametersChosenResultKey, chosenResult.ToJson() }
            }, cToken);

            buttonsData.Add(nextCommand.Id.ToString(), date.Key);
        }

        if (buttonsData.Count == 0)
        {
            _log.Info($"Available dates for '{city.Name}' with Id '{kdmidId.Id}' were not found for the chat '{message.Id}'.");
        }
        else
        {
            await _botClient.SendButtons(new(message, new($"Available dates for '{city.Name}' with Id '{kdmidId.Id}'.", buttonsData, 1)), cToken);
        }
    }
    public async Task SendConfirmationResult(Message message, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();
        var chosenResult = command.Parameters[BotCommandParametersChosenResultKey].FromJson<ChosenDateResult>();

        try
        {
            await _kdmidRequestService.ConfirmChosenDate(city, kdmidId, chosenResult, cToken);
            await _botClient.SendText(new(message, new($"'{chosenResult.ChosenKey}' for '{city.Name}' with Id '{kdmidId.Id}' has been confirmed.")), cToken);
            await _repository.Clear(message.Chat.Id, command.Id, cToken);
        }
        catch (UserInvalidOperationException exception)
        {
            throw new UserInvalidOperationException($"The confirmation of '{chosenResult.ChosenKey}' for '{city.Name}' with Id '{kdmidId.Id}' was failed for the chat '{message.Id}'. Reason: {exception.Message}");
        }
        catch
        {
            throw;
        }
    }
    public Task SendInfo(Message message, Command command, CancellationToken cToken)
    {
        return _botClient.SendText(new(message, new("You have to receive the details of your embassies processes, but it has not been implemented yet.")), cToken);
    }

    public Task SendAskResponse(Message message, Command command, CancellationToken cToken)
    {
        Text text = command.Parameters.Count == 0
            ? new("To send your message to the developer, use the double quotes.")
            : command.Parameters.TryGetValue(CommandParameters.Message, out var response)
                ? new(response)
                : throw new ArgumentException("The message for the developer is not specified.");

        return _botClient.SendText(new(message, text), cToken);
    }
    public Task SendAnswerResponse(Message message, Command command, CancellationToken cToken)
    {
        if (!command.Parameters.TryGetValue(CommandParameters.ChatId, out var targetChatId))
            throw new ArgumentException("The chatId for the user is not specified.");

        if (!command.Parameters.TryGetValue(CommandParameters.Message, out var text))
            throw new ArgumentException("The message for the user is not specified.");

        var responseMessage = new Message(null, new(targetChatId));
        return _botClient.SendText(new(responseMessage, new(text)), cToken);
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
            if (attempts.Day == day && attempts.Count >= KdmidRequestAttemptsLimit)
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
