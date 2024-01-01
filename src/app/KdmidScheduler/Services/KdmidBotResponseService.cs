using System.Text.Json;

using KdmidScheduler.Abstractions.Interfaces;
using KdmidScheduler.Abstractions.Models.v1;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Services;

public sealed class KdmidBotResponseService(
    IBotClient client,
    IBotCommandsStore commandStore,
    IKdmidService kdmidService) : IBotResponseService
{
    private const string StartCommand = "start";
    private const string SendAvailableDatesCommand = "sendAvailableDates";
    private const string SendConfirmResultCommand = "sendConfirmResult";

    private static readonly string CityKey = typeof(City).FullName!;
    private static readonly string KdmidIdKey = typeof(Identifier).FullName!;
    private static readonly string ChosenResultKey = typeof(ChosenDateResult).FullName!;

    private readonly IBotClient _client = client;
    private readonly IKdmidService _kdmidService = kdmidService;
    private readonly IBotCommandsStore _commandStore = commandStore;

    public Task CreateResponse(string chatId, string commandName, CancellationToken cToken) => commandName switch
    {
        StartCommand => SendAvailableEmbassies(chatId, cToken),
        _ => throw new NotSupportedException($"The command '{commandName}' is not supported.")
    };
    public Task CreateResponse(string chatId, BotCommand command, CancellationToken cToken) => command.Name switch
    {
        SendAvailableDatesCommand => SendAvailableDates(chatId, command.Parameters, cToken),
        SendConfirmResultCommand => SendConfirmResult(chatId, command.Parameters, cToken),
        _ => throw new NotSupportedException($"The command '{command}' is not supported.")
    };

    private async Task SendAvailableEmbassies(string chatId, CancellationToken cToken)
    {
        var availableCities = _kdmidService.GetAvailableCities(cToken);

        var buttonsData = new Dictionary<string, string>(availableCities.Length);

        foreach (var city in availableCities)
        {
            var nextCommand = new BotCommand(SendAvailableDatesCommand, new()
            {
                { CityKey, JsonSerializer.Serialize(city)}
            });

            var nextCommandId = await _commandStore.Create(chatId, nextCommand, cToken);

            buttonsData.Add(nextCommandId.ToString(), city.Name);
        }
        
        var buttonsArgs = new ButtonsEventArgs(chatId, new("Available embassies",3, buttonsData));
        
        await _client.SendButtons(buttonsArgs, cToken);
    }
    private async Task SendIdentifiersWebForm(string chatId, IReadOnlyDictionary<string, string> parameters, CancellationToken cToken)
    {
        await _client.SendWebForm(chatId, new Identifier(), cToken);
    }
    private async Task SendAvailableDates(string chatId, IReadOnlyDictionary<string, string> parameters, CancellationToken cToken)
    {
        if (!parameters.ContainsKey(KdmidIdKey))
        {
            await SendIdentifiersWebForm(chatId, parameters, cToken);
        }
        else
        {
            var kdmidId =
                JsonSerializer.Deserialize<Identifier>(parameters[KdmidIdKey])
                ?? throw new ArgumentException("The kdmidId is not specified.");

            var city =
                JsonSerializer.Deserialize<City>(parameters[CityKey])
                ?? throw new ArgumentException("The city is not specified.");

            var availableDatesResult = await _kdmidService.GetAvailableDates(city, kdmidId, cToken);

            var buttonsData = new Dictionary<string, string>(availableDatesResult.Dates.Count);

            foreach (var date in availableDatesResult.Dates)
            {
                var nextCommand = new BotCommand(SendConfirmResultCommand, new()
                {
                    { CityKey, parameters[CityKey] },
                    { KdmidIdKey, parameters[KdmidIdKey] },
                    { ChosenResultKey, JsonSerializer.Serialize(new ChosenDateResult(availableDatesResult.FormData, date.Key, date.Value)) }
                });
                
                var nextCommandId = await _commandStore.Create(chatId, nextCommand, cToken);

                buttonsData.Add(nextCommandId.ToString(), date.Value);
            }

            var buttonsArgs = new ButtonsEventArgs(chatId, new("Available dates", 1, buttonsData));

            await _client.SendButtons(buttonsArgs, cToken);
        }
    }
    private async Task SendConfirmResult(string chatId, IReadOnlyDictionary<string, string> parameters, CancellationToken cToken)
    {
        var city =
            JsonSerializer.Deserialize<City>(parameters[CityKey])
            ?? throw new ArgumentException("The city is not specified.");

        var kdmidId =
            JsonSerializer.Deserialize<Identifier>(parameters[KdmidIdKey])
            ?? throw new ArgumentException("The kdmidId is not specified.");

        var chosenResult =
            JsonSerializer.Deserialize<ChosenDateResult>(parameters[ChosenResultKey])
            ?? throw new ArgumentException("The chosenResult is not specified.");

        var confirmResult = await _kdmidService.ConfirmDate(city, kdmidId, chosenResult, cToken);

        if (confirmResult.IsSuccess)
        {
            await _client.SendText(chatId, "The date is confirmed.", cToken);
        }
        else
        {
            await _client.SendText(chatId, confirmResult.Message, cToken);
        }
    }
}
