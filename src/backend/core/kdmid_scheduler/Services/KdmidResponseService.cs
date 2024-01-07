using System.Text.Json;
using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Core.v1;
using KdmidScheduler.Abstractions.Models.Settings;

using Microsoft.Extensions.Options;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Services;

public sealed class KdmidResponseService(
    IBotClient botClient,
    IBotCommandsStore botCommandsStore,
    IKdmidRequestService kdmidRequestService,
    IOptions<KdmidSettings> options
    ) : IKdmidResponseService
{
    private static readonly string CityKey = typeof(City).FullName!;
    private static readonly string KdmidIdKey = typeof(Identifier).FullName!;
    private static readonly string ChosenResultKey = typeof(ChosenDateResult).FullName!;

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

        var webAppData = new Dictionary<string, Uri>(myCities.Length);

        foreach (var city in availableCities)
        {
            var command = await _botCommandsStore.Create(chatId, IKdmidResponseService.SendAvailableDatesCommand, new()
            {
                { CityKey, JsonSerializer.Serialize(city, _jsonSerializerOptions)}
            }, cToken);

            var uri = new Uri($"{_kdmidSettings.WebAppUrl}?chatId={chatId}&commandId={command.Id}");

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
        await _botCommandsStore.Clear(chatId, cToken);

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
            JsonSerializer.Deserialize<Identifier>(command.Parameters[KdmidIdKey], _jsonSerializerOptions)
            ?? throw new ArgumentException("The kdmidId is not specified.");

        var availableDatesResult = await _kdmidRequestService.GetAvailableDates(city, kdmidId, cToken);

        var buttonsData = new Dictionary<string, string>(availableDatesResult.Dates.Count);

        foreach (var date in availableDatesResult.Dates)
        {
            var nextCommand = await _botCommandsStore.Create(chatId, IKdmidResponseService.SendConfirmResultCommand, new()
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
            JsonSerializer.Deserialize<Identifier>(command.Parameters[KdmidIdKey], _jsonSerializerOptions)
        ?? throw new ArgumentException("The kdmidId is not specified.");

        var chosenResult =
            JsonSerializer.Deserialize<ChosenDateResult>(command.Parameters[ChosenResultKey], _jsonSerializerOptions)
            ?? throw new ArgumentException("The chosenResult is not specified.");

        var confirmResult = await _kdmidRequestService.ConfirmChosenDate(city, kdmidId, chosenResult, cToken);

        if (confirmResult.IsSuccess)
        {
            var messageArgs = new MessageEventArgs(chatId, new("The date is confirmed."));
            await _botClient.SendMessage(messageArgs, cToken);
        }
        else
        {
            var messageArgs = new MessageEventArgs(chatId, new(confirmResult.Message));
            await _botClient.SendMessage(messageArgs, cToken);
        }
    }
}
