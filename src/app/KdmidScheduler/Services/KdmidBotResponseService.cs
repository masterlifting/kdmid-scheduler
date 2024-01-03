using System.Text.Json;

using KdmidScheduler.Abstractions.Interfaces;
using KdmidScheduler.Abstractions.Models.v1;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Services;

public sealed class KdmidBotResponseService(IBotClient botClient, IBotCommandsStore commandStore, IKdmidService kdmidService) : IBotResponseService
{
    private const string StartCommand = "start";
    private const string SendAvailableDatesCommand = "sendAvailableDates";
    private const string SendConfirmResultCommand = "sendConfirmResult";

    private static readonly string CityKey = typeof(City).FullName!;
    private static readonly string KdmidIdKey = typeof(Identifier).FullName!;
    private static readonly string ChosenResultKey = typeof(ChosenDateResult).FullName!;

    private readonly IBotClient _botClient = botClient;
    private readonly IKdmidService _kdmidService = kdmidService;
    private readonly IBotCommandsStore _commandStore = commandStore;

    public Task CreateResponse(string chatId, string commandName, CancellationToken cToken) => commandName switch
    {
        StartCommand => SendAvailableEmbassies(chatId, cToken),
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

        var buttonsArgs = new ButtonsEventArgs(chatId, new("Available embassies", 3, buttonsData));

        await _botClient.SendButtons(buttonsArgs, cToken);
    }
    private async Task SendAvailableDates(string chatId, BotCommand command, CancellationToken cToken)
    {
        var city =
            JsonSerializer.Deserialize<City>(command.Parameters[CityKey])
            ?? throw new ArgumentException("The city is not specified.");

        var kdmidId =
            JsonSerializer.Deserialize<Identifier>(command.Parameters[KdmidIdKey])
            ?? throw new ArgumentException("The kdmidId is not specified.");

        var availableDatesResult = await _kdmidService.GetAvailableDates(city, kdmidId, cToken);

        var buttonsData = new Dictionary<string, string>(availableDatesResult.Dates.Count);

        foreach (var date in availableDatesResult.Dates)
        {
            var nextCommand = new BotCommand(SendConfirmResultCommand, new()
                {
                    { CityKey, command.Parameters[CityKey] },
                    { KdmidIdKey, command.Parameters[KdmidIdKey] },
                    { ChosenResultKey, JsonSerializer.Serialize(new ChosenDateResult(availableDatesResult.FormData, date.Key, date.Value)) }
                });

            var nextCommandId = await _commandStore.Create(chatId, nextCommand, cToken);

            buttonsData.Add(nextCommandId.ToString(), date.Value);
        }

        var buttonsArgs = new ButtonsEventArgs(chatId, new("Available dates", 1, buttonsData));

        await _botClient.SendButtons(buttonsArgs, cToken);
    }
    private async Task SendConfirmResult(string chatId, BotCommand command, CancellationToken cToken)
    {
        var city =
            JsonSerializer.Deserialize<City>(command.Parameters[CityKey])
            ?? throw new ArgumentException("The city is not specified.");

        var kdmidId =
            JsonSerializer.Deserialize<Identifier>(command.Parameters[KdmidIdKey])
            ?? throw new ArgumentException("The kdmidId is not specified.");

        var chosenResult =
            JsonSerializer.Deserialize<ChosenDateResult>(command.Parameters[ChosenResultKey])
            ?? throw new ArgumentException("The chosenResult is not specified.");

        var confirmResult = await _kdmidService.ConfirmDate(city, kdmidId, chosenResult, cToken);

        if (confirmResult.IsSuccess)
        {
            await _botClient.SendText(chatId, "The date is confirmed.", cToken);
        }
        else
        {
            await _botClient.SendText(chatId, confirmResult.Message, cToken);
        }
    }
}
