using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Core.v1;
using KdmidScheduler.Abstractions.Models.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Net.Shared.Abstractions.Models.Exceptions;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;
using Net.Shared.Bots.Abstractions.Models.Exceptions;
using Net.Shared.Bots.Abstractions.Models.Settings;
using Net.Shared.Extensions.Logging;
using Net.Shared.Extensions.Serialization.Json;

using static KdmidScheduler.Abstractions.Constants;
using static Net.Shared.Bots.Abstractions.Constants;

namespace KdmidScheduler.Services;

public sealed class KdmidResponseService(
    IOptions<BotConnectionSettings> botOptions,
    IOptions<KdmidSettings> kdmidOptions,
    ILogger<KdmidResponseService> logger,
    IBotClient botClient,
    IBotCommandsStore botCommandsStore,
    IKdmidRequestService kdmidRequestService
    ) : IKdmidResponseService
{
    public static readonly string CityKey = typeof(City).FullName!;
    public static readonly string KdmidIdKey = typeof(KdmidId).FullName!;
    public static readonly string ChosenResultKey = typeof(ChosenDateResult).FullName!;
    public static readonly string AttemptsKey = typeof(Attempts).FullName!;
    public const byte AttemptsLimit = 23;

    private readonly ILogger<KdmidResponseService> _logger = logger;

    private readonly KdmidSettings _kdmidSettings = kdmidOptions.Value;
    private readonly BotConnectionSettings _botSettings = botOptions.Value;
    
    private readonly IBotClient _botClient = botClient;
    private readonly IBotCommandsStore _botCommandsStore = botCommandsStore;
    private readonly IKdmidRequestService _kdmidRequestService = kdmidRequestService;

    public async Task SendAvailableEmbassies(string chatId, CancellationToken cToken)
    {
        var supportedCities = _kdmidRequestService.GetSupportedCities(cToken);

        var webAppData = new Dictionary<string, Uri>(supportedCities.Length);

        foreach (var city in supportedCities)
        {
            var command = await _botCommandsStore.Create(chatId, KdmidBotCommands.AddAvailableEmbassy, new()
            {
                { CityKey, city.ToJson()}
            }, cToken);

            var uri = new Uri($"{_kdmidSettings.WebAppUrl}/kdmidId?chatId={chatId}&commandId={command.Id}");
            
            _logger.Debug($"Created a command with id {command.Id} for chat {chatId}.");

            webAppData.Add(city.Name, uri);
        }

        if (webAppData.Count == 0)
        {
            await _botClient.SendMessage(new(chatId, new("There are no available embassies.")), cToken);
        }
        else
        {
            var webAppArgs = new WebAppEventArgs(chatId, new("Available embassies", 3, webAppData));
            await _botClient.SendWebApp(webAppArgs, cToken);
        }
    }
    public async Task AddAvailableEmbassy(string chatId, BotCommand command, CancellationToken cToken)
    {
        var kdmidId = command.Parameters[KdmidIdKey].FromJson<KdmidId>();

        try
        {
            kdmidId.Validate();

            var nexCommand = await _botCommandsStore.Create(chatId, KdmidBotCommands.SendAvailableDates, new()
            {
                { CityKey, command.Parameters[CityKey] },
                { KdmidIdKey, command.Parameters[KdmidIdKey] }
            }, cToken);
            
            await _botClient.SendMessage(new(chatId, new("The embassy is added to your list.")), cToken);
            
            var city = command.Parameters[CityKey].FromJson<City>();
            await _botClient.SendMessage(new(_botSettings.AdminId, new($"The embassy for '{city.Name}' is added to the list of the user '{chatId}'.")), cToken);
        }
        catch (UserInvalidOperationException exception)
        {
            throw new BotUserInvalidOperationException(exception.Message);
        }
        catch
        {
            throw;
        }
    }
    public async Task SendMyEmbassies(string chatId, CancellationToken cToken)
    {
        var supportedCities = _kdmidRequestService.GetSupportedCities(cToken);

        var commands = await _botCommandsStore.Get(chatId, cToken);

        var availableCommands = commands
            .Where(x => x.Parameters.ContainsKey(KdmidIdKey))
            .ToArray();

        var webAppData = new Dictionary<string, Uri>(availableCommands.Length);

        foreach (var command in availableCommands)
        {
            var city = command.Parameters[CityKey].FromJson<City>();

            var uri = new Uri($"{_kdmidSettings.WebAppUrl}/embassy?chatId={chatId}&commandId={command.Id}");

            webAppData.Add(city.Name, uri);
        }

        if (webAppData.Count == 0)
        {
            var messageArgs = new MessageEventArgs(chatId, new("You have no embassies in your list."));
            await _botClient.SendMessage(messageArgs, cToken);
        }
        else
        {
            var webAppArgs = new WebAppEventArgs(chatId, new("My embassies", 3, webAppData));
            await _botClient.SendWebApp(webAppArgs, cToken);
        }
    }
    public async Task SendAvailableDates(string chatId, BotCommand command, CancellationToken cToken)
    {
        var city = command.Parameters[CityKey].FromJson<City>();
        var kdmidId = command.Parameters[KdmidIdKey].FromJson<KdmidId>();

        AvailableDatesResult availableDatesResult;
        
        try
        {
            kdmidId.Validate();
            
            await AddAttempt(chatId, command, city, cToken);

            availableDatesResult = await _kdmidRequestService.GetAvailableDates(city, kdmidId, cToken);
        }
        catch (UserInvalidOperationException exception)
        {
            throw new BotUserInvalidOperationException(exception.Message);
        }
        catch
        {
            throw;
        }

        var buttonsData = new Dictionary<string, string>(availableDatesResult.Dates.Count);

        foreach (var date in availableDatesResult.Dates)
        {
            var chosenResult = new ChosenDateResult(availableDatesResult.FormData, date.Key, date.Value);
            var nextCommand = await _botCommandsStore.Create(chatId, KdmidBotCommands.SendConfirmResult, new()
            {
                { CityKey, command.Parameters[CityKey] },
                { KdmidIdKey, command.Parameters[KdmidIdKey] },
                { ChosenResultKey, chosenResult.ToJson() }
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
        var city =command.Parameters[CityKey].FromJson<City>();
        var kdmidId =command.Parameters[KdmidIdKey].FromJson<KdmidId>();
        var chosenResult = command.Parameters[ChosenResultKey].FromJson<ChosenDateResult>();

        try
        {
            await _kdmidRequestService.ConfirmChosenDate(city, kdmidId, chosenResult, cToken);

            var messageArgs = new MessageEventArgs(chatId, new("The date is confirmed."));
            await _botClient.SendMessage(messageArgs, cToken);
        }
        catch (UserInvalidOperationException exception)
        {
            throw new BotUserInvalidOperationException(exception.Message);
        }
        catch
        {
            throw;
        }
        finally
        {
            await _botCommandsStore.Delete(chatId, command.Id, cToken);
        }
    }
    public Task SendAskResponse(string chatId, BotCommand command, CancellationToken cToken)
    {
        MessageEventArgs messageArgs = command.Parameters.Count == 0
            ? new(chatId, new("To send your message to the developer, use the double quotes."))
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

    private async Task AddAttempt(string chatId, BotCommand command, City city, CancellationToken cToken)
    {
        var attempts = command.Parameters.TryGetValue(AttemptsKey, out var attemptsValue)
            ? attemptsValue.FromJson<Attempts>()
            : null;

        var day = DateTime.UtcNow.AddHours(city.TimeShift).DayOfYear;

        if (attempts is null)
        {
            attempts = new Attempts(day, 1);
            command.Parameters.Add(AttemptsKey, attempts.ToJson());
        }
        else
        {
            if (attempts.Day == day && attempts.Count >= AttemptsLimit)
                throw new BotUserInvalidOperationException($"You have reached the limit of attempts per day ({AttemptsLimit}) for the city {city.Name}.");

            attempts = new Attempts(attempts.Day, (byte)(attempts.Count + 1));
            
            command.Parameters[AttemptsKey] = attempts.ToJson();
        }

        await _botCommandsStore.Update(chatId, command.Id, command, cToken);
    }

}
