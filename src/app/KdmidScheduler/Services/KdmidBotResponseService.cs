using System.Text.Json;

using KdmidScheduler.Abstractions.Interfaces;
using KdmidScheduler.Abstractions.Models.v1;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Services;

public sealed class KdmidBotResponseService(IBotClient botClient, IBotCommandsStore commandStore, IKdmidService kdmidService) : IBotResponseService
{
    private const string StartCommand = "start";
    private const string MineCommand = "mine";
    private const string SendAvailableDatesCommand = "sendAvailableDates";
    private const string SendConfirmResultCommand = "sendConfirmResult";

    private static readonly string CityKey = typeof(City).FullName!;
    private static readonly string KdmidIdKey = typeof(Identifier).FullName!;
    private static readonly string ChosenResultKey = typeof(ChosenDateResult).FullName!;

    private readonly IBotClient _botClient = botClient;
    private readonly IKdmidService _kdmidService = kdmidService;
    private readonly IBotCommandsStore _commandStore = commandStore;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { 
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task CreateResponse(string chatId, string commandName, CancellationToken cToken) => commandName switch
    {
        StartCommand => SendAvailableEmbassies(chatId, cToken),
        MineCommand => SendMyEmbassies(chatId, cToken),
        _ => throw new NotSupportedException($"The command '{commandName}' is not supported.")
    };
    public Task CreateResponse(string chatId, BotCommand command, CancellationToken cToken) => command.Name switch
    {
        SendAvailableDatesCommand => SendAvailableDates(chatId, command, cToken),
        SendConfirmResultCommand => SendConfirmResult(chatId, command, cToken),
        _ => throw new NotSupportedException($"The command '{command}' is not supported.")
    };

    private async Task SendAvailableEmbassies(string chatId, CancellationToken cToken)
    {
        var supportedCities = _kdmidService.GetSupportedCities(cToken);

        var commands = await _commandStore.Get(chatId, cToken);

        var myCities = commands
            .Where(x => x.Parameters.ContainsKey(KdmidIdKey))
            .Select(x => JsonSerializer.Deserialize<City>(x.Parameters[CityKey], _jsonSerializerOptions))
            .Select(x => x!.Code)
            .ToArray();

        var availableCities = supportedCities.ExceptBy(myCities, x => x.Code);

        var webAppData = new Dictionary<string, Uri>(myCities.Length);

        foreach (var city in availableCities)
        {
            var command = await _commandStore.Create(chatId, SendAvailableDatesCommand, new()
            {
                { CityKey, JsonSerializer.Serialize(city, _jsonSerializerOptions)}
            }, cToken);

            var uri = new Uri($"https://kdmid-scheduler.netlify.app/identifier?chatId={chatId}&commandId={command.Id}");

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
    private async Task SendMyEmbassies(string chatId, CancellationToken cToken)
    {
        var supportedCities = _kdmidService.GetSupportedCities(cToken);

        var commands = await _commandStore.Get(chatId, cToken);

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
    private async Task SendAvailableDates(string chatId, BotCommand command, CancellationToken cToken)
    {
        var city =
            JsonSerializer.Deserialize<City>(command.Parameters[CityKey], _jsonSerializerOptions)
            ?? throw new ArgumentException("The city is not specified.");

        var kdmidId =
            JsonSerializer.Deserialize<Identifier>(command.Parameters[KdmidIdKey], _jsonSerializerOptions)
            ?? throw new ArgumentException("The kdmidId is not specified.");

        var availableDatesResult = await _kdmidService.GetAvailableDates(city, kdmidId, cToken);

        var buttonsData = new Dictionary<string, string>(availableDatesResult.Dates.Count);

        foreach (var date in availableDatesResult.Dates)
        {
            var nextCommand = await _commandStore.Create(chatId, SendConfirmResultCommand, new()
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
    private async Task SendConfirmResult(string chatId, BotCommand command, CancellationToken cToken)
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

        var confirmResult = await _kdmidService.ConfirmDate(city, kdmidId, chosenResult, cToken);

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
