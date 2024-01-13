using System.Text.Json;

using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Core.v1;
using KdmidScheduler.Abstractions.Models.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;
using Net.Shared.Extensions.Logging;

using static KdmidScheduler.Abstractions.Constants;
using static Net.Shared.Bots.Abstractions.Constants;

namespace KdmidScheduler.Services;

public sealed class KdmidResponseService(
    ILogger<KdmidResponseService> logger,
    IBotClient botClient,
    IBotCommandsStore botCommandsStore,
    IKdmidRequestService kdmidRequestService,
    IOptions<KdmidSettings> options
    ) : IKdmidResponseService
{
    public static readonly string CityKey = typeof(City).FullName!;
    public static readonly string KdmidIdKey = typeof(KdmidId).FullName!;
    public static readonly string ChosenResultKey = typeof(ChosenDateResult).FullName!;

    private readonly ILogger<KdmidResponseService> _logger = logger;
    private readonly IBotClient _botClient = botClient;
    private readonly IBotCommandsStore _botCommandsStore = botCommandsStore;
    private readonly IKdmidRequestService _kdmidRequestService = kdmidRequestService;
    private readonly KdmidSettings _kdmidSettings = options.Value;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task SendAvailableEmbassies(string chatId, CancellationToken cToken)
    {
        await _botCommandsStore.Clear(chatId, cToken);

        var supportedCities = _kdmidRequestService.GetSupportedCities(cToken);
        var commands = await _botCommandsStore.Get(chatId, cToken);
        var myCities = commands
            .Where(x => x.Parameters.ContainsKey(KdmidIdKey))
            .Select(x => JsonSerializer.Deserialize<City>(x.Parameters[CityKey], _jsonSerializerOptions))
            .Select(x => x!.Code)
            .ToArray();

        var availableCities = supportedCities.ExceptBy(myCities, x => x.Code);

        if (!availableCities.Any())
        {
            availableCities = supportedCities;
        }

        var webAppData = new Dictionary<string, Uri>(myCities.Length);

        foreach (var city in availableCities)
        {
            var command = await _botCommandsStore.Create(chatId, KdmidBotCommands.SendAvailableDates, new()
            {
                { CityKey, JsonSerializer.Serialize(city, _jsonSerializerOptions)}
            }, cToken);

            var uri = new Uri($"{_kdmidSettings.WebAppUrl}/kdmidId?chatId={chatId}&commandId={command.Id}");
            _logger.Debug($"Created a command with id {command.Id} for chat {chatId}.");

            webAppData.Add(city.Name, uri);
        }

        if (webAppData.Count == 0)
        {
            var messageArgs = new MessageEventArgs(chatId, new("There are no available embassies."));
            await _botClient.SendMessage(messageArgs, cToken);
        }
        else
        {
            var webAppArgs = new WebAppEventArgs(chatId, new("Available embassies", 3, webAppData));
            await _botClient.SendWebApp(webAppArgs, cToken);
        }
    }
    public async Task SendMyEmbassies(string chatId, CancellationToken cToken)
    {
        var supportedCities = _kdmidRequestService.GetSupportedCities(cToken);

        var commands = await _botCommandsStore.Get(chatId, cToken);

        var availableCommands = commands
            .Where(x => x.Parameters.ContainsKey(KdmidIdKey))
            .ToArray();

        var buttonsData = new Dictionary<string, string>(availableCommands.Length);

        foreach (var command in availableCommands)
        {
            var city =
                JsonSerializer.Deserialize<City>(command.Parameters[CityKey], _jsonSerializerOptions)
                ?? throw new ArgumentException("The city is not specified.");

            buttonsData.Add(command.Id.ToString(), city.Name);
        }

        if (buttonsData.Count == 0)
        {
            var messageArgs = new MessageEventArgs(chatId, new("You have no embassies in your list."));
            await _botClient.SendMessage(messageArgs, cToken);
        }
        else
        {
            var buttonsArgs = new ButtonsEventArgs(chatId, new("My embassies", 3, buttonsData));
            await _botClient.SendButtons(buttonsArgs, cToken);
        }
    }
    public async Task SendAvailableDates(string chatId, BotCommand command, CancellationToken cToken)
    {
        var city =
            JsonSerializer.Deserialize<City>(command.Parameters[CityKey], _jsonSerializerOptions)
            ?? throw new ArgumentException("The city is not specified.");

        var kdmidId =
            JsonSerializer.Deserialize<KdmidId>(command.Parameters[KdmidIdKey], _jsonSerializerOptions)
            ?? throw new ArgumentException("The kdmidId is not specified.");

        var availableDatesResult = await _kdmidRequestService.GetAvailableDates(city, kdmidId, cToken);

        var buttonsData = new Dictionary<string, string>(availableDatesResult.Dates.Count);

        foreach (var date in availableDatesResult.Dates)
        {
            var nextCommand = await _botCommandsStore.Create(chatId, KdmidBotCommands.SendConfirmResult, new()
            {
                { CityKey, command.Parameters[CityKey] },
                { KdmidIdKey, command.Parameters[KdmidIdKey] },
                    { ChosenResultKey, JsonSerializer.Serialize(new ChosenDateResult(availableDatesResult.FormData, date.Key, date.Value), _jsonSerializerOptions) }
                }, cToken);

            buttonsData.Add(nextCommand.Id.ToString(), date.Value);
        }

        if (buttonsData.Count == 0)
        {
            var messageArgs = new MessageEventArgs(chatId, new("There are no available dates."));
            await _botClient.SendMessage(messageArgs, cToken);
        }
        else
        {
            var buttonsArgs = new ButtonsEventArgs(chatId, new("Choose a date", 1, buttonsData));
            await _botClient.SendButtons(buttonsArgs, cToken);
        }
    }
    public async Task SendConfirmationResult(string chatId, BotCommand command, CancellationToken cToken)
    {
        var city =
            JsonSerializer.Deserialize<City>(command.Parameters[CityKey], _jsonSerializerOptions)
            ?? throw new ArgumentException("The city is not specified.");

        var kdmidId =
            JsonSerializer.Deserialize<KdmidId>(command.Parameters[KdmidIdKey], _jsonSerializerOptions)
        ?? throw new ArgumentException("The kdmidId is not specified.");

        var chosenResult =
            JsonSerializer.Deserialize<ChosenDateResult>(command.Parameters[ChosenResultKey], _jsonSerializerOptions)
            ?? throw new ArgumentException("The chosenResult is not specified.");

        await _kdmidRequestService.ConfirmChosenDate(city, kdmidId, chosenResult, cToken);

        var messageArgs = new MessageEventArgs(chatId, new("The date is confirmed."));
        await _botClient.SendMessage(messageArgs, cToken);
    }
    public Task SendAskResponse(string chatId, BotCommand command, CancellationToken cToken)
    {
        MessageEventArgs messageArgs = command.Parameters.Count == 0
            ? new(chatId, new("To send your message to the developer, type the message in double quotes."))
            : command.Parameters.TryGetValue(CommandParameters.Message, out var text)
                ? new(chatId, new(text))
                : throw new ArgumentException("The message for the developer is not specified.");

        return _botClient.SendMessage(messageArgs, cToken);
    }
    public Task SendAnswerResponse(string chatId, BotCommand command, CancellationToken cToken)
    {
        if (!command.Parameters.TryGetValue(CommandParameters.ChatId, out var targetChatId))
            throw new ArgumentException("The chatId for the user is not specified.");

        if (!command.Parameters.TryGetValue(CommandParameters.Message, out var text))
            throw new ArgumentException("The message for the user is not specified.");

        var adminMessageArgs = new MessageEventArgs(targetChatId, new(text));
        return _botClient.SendMessage(adminMessageArgs, cToken);
    }
}
