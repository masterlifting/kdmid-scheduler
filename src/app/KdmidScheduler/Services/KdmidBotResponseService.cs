using System.Text.Json;

using KdmidScheduler.Abstractions.Interfaces;
using KdmidScheduler.Abstractions.Models;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Services;

public sealed class KdmidBotResponseService(
    IBotClient botClient,
    IBotCommandsStore botCommandStore,
    IKdmidService kdmidService) : IBotResponseService
{

    private readonly IBotClient _botClient = botClient;
    private readonly IKdmidService _kdmidService = kdmidService;
    private readonly IBotCommandsStore _botCommandStore = botCommandStore;

    public Task CreateResponse(string chatId, BotCommand command, CancellationToken cToken) => command.Name switch
    {
        "start" => SendAvailableEmbassies(chatId, cToken),
        "sendAvailableDates" => SendAvailableDates(chatId, command.Parameters, cToken),
        "sendConfirmResult" => SendConfirmResult(chatId, command.Parameters, cToken),
        _ => throw new NotSupportedException($"The command '{command}' is not supported.")
    };

    private async Task SendAvailableEmbassies(string chatId, CancellationToken cToken)
    {
        var availableCities = _kdmidService.GetAvailableCities(cToken);

        var clientButtons = new Dictionary<string, string>(availableCities.Length);

        foreach (var city in availableCities)
        {
            var jsonCity = JsonSerializer.Serialize(city);

            var nextCommand = new BotCommand($"/sendAvailableDates?city={jsonCity}");

            var nextCommandId = await _botCommandStore.SetCommand(chatId, nextCommand, cToken);

            clientButtons.Add(nextCommandId.ToString(), city.Name);
        }

        await _botClient.SendButtons(chatId, clientButtons, cToken);
    }
    private async Task SendIdentifiersWebForm(string chatId, IReadOnlyDictionary<string, string> parameters, CancellationToken cToken)
    {
        await _botClient.SendWebForm(chatId, new Identifier(), cToken);
    }
    private async Task SendAvailableDates(string chatId, IReadOnlyDictionary<string, string> parameters, CancellationToken cToken)
    {
        var kdmidId = JsonSerializer.Deserialize<Identifier>(parameters["kdmidId"]);

        if (kdmidId is null)
        {
            await SendIdentifiersWebForm(chatId, parameters, cToken);
        }
        else
        {
            var city =
                JsonSerializer.Deserialize<City>(parameters["city"])
                ?? throw new ArgumentException("The city is not specified.");

            var availableDatesResult = await _kdmidService.GetAvailableDates(city, kdmidId, cToken);

            var clientButtons = new Dictionary<string, string>(availableDatesResult.Dates.Count);

            foreach (var date in availableDatesResult.Dates)
            {
                var jsonChosenResult = JsonSerializer.Serialize(new ChosenDateResult(availableDatesResult.FormData, date.Key, date.Value));

                var nextCommand = new BotCommand($"/sendConfirmResult?city={parameters["city"]}&kdmidId={parameters["kdmidId"]}&choice={jsonChosenResult}");
                var nextCommandId = await _botCommandStore.SetCommand(chatId, nextCommand, cToken);

                clientButtons.Add(nextCommandId.ToString(), date.Value);
            }

            await _botClient.SendButtons(chatId, clientButtons, cToken);
        }
    }
    private async Task SendConfirmResult(string chatId, IReadOnlyDictionary<string, string> parameters, CancellationToken cToken)
    {
        var city =
            JsonSerializer.Deserialize<City>(parameters["city"])
            ?? throw new ArgumentException("The city is not specified.");

        var kdmidId =
            JsonSerializer.Deserialize<Identifier>(parameters["kdmidId"])
            ?? throw new ArgumentException("The kdmidId is not specified.");

        var chosenResult =
            JsonSerializer.Deserialize<ChosenDateResult>(parameters["choice"])
            ?? throw new ArgumentException("The chosenResult is not specified.");

        if(chosenResult is null)
        {
            throw new InvalidOperationException("The chosenResult is not specified.");
        }
            
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
