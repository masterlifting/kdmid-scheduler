using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Models.Core.v1.KdmidWebApi;

using Microsoft.Extensions.Logging;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Extensions.Logging;
using Net.Shared.Extensions.Serialization.Json;

using static KdmidScheduler.Abstractions.Constants;
using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Services;

public class KdmidBotApi(
    ILogger<KdmidBotApi> logger,
    IBotClient cotClient,
    IBotResponse botResponse,
    IBotCommandsStore botCommandsStore,
    IKdmidRequestService kdmidRequestService
    ) : IKdmidBotApi
{
    private readonly ILogger _log = logger;
    private readonly IBotClient _botClient = cotClient;
    private readonly IBotResponse _botResponse = botResponse;
    private readonly IBotCommandsStore _botCommandsStore = botCommandsStore;
    private readonly IKdmidRequestService _kdmidRequestService = kdmidRequestService;

    #region Client
    public async Task Listen(CancellationToken cToken)
    {
        _log.Info("Bot client going to listen.");
        await _botClient.Listen(cToken);
        _log.Info("Bot client is listening.");
    }
    public async Task Listen(Uri uri, CancellationToken cToken)
    {
        _log.Info($"Bot client going to listen on {uri}.");
        await _botClient.Listen(uri, cToken);
        _log.Info($"Bot client is listening on {uri}.");
    }
    public async Task Receive(StreamReader reader, CancellationToken cToken)
    {
        _log.Debug("Bot client going to receive data.");
        var data = await reader.ReadToEndAsync(cToken);

        if (string.IsNullOrWhiteSpace(data))
            throw new InvalidOperationException("Received data is empty.");

        await _botClient.Receive(data, cToken);
        _log.Debug("Bot client received data.");
    }
    #endregion

    #region Queries
    public Task<CityGetDto[]> GetCities(CancellationToken cToken)
    {
        var cities = _kdmidRequestService.GetSupportedCities(cToken);

        var result = cities
            .Select(x => new CityGetDto(x.Code, x.Name))
            .ToArray();

        _log.Debug($"Cities: {string.Join("\n", result.ToJson())}.");
    

        return Task.FromResult(result);
    }
    public async Task<CommandGetDto> GetCommand(string chatId, string commandId, CancellationToken cToken)
    {
        var commandIdGuid = Guid.Parse(commandId);
        var command = await _botCommandsStore.Get(chatId, commandIdGuid, cToken);

        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();
        var attempts = command.Parameters.TryGetValue(BotCommandParametersAttemptsKey, out var attemptsStr)
            ? attemptsStr.FromJson<Attempts>().Count
            : (byte)0;

        return new CommandGetDto(commandId, city.Name, kdmidId.Id, kdmidId.Cd, kdmidId.Ems, attempts);
    }
    public async Task<CommandGetDto[]> GetCommands(string chatId, string? names, string? cityCode, CancellationToken cToken)
    {
        var commands = await _botCommandsStore.Get(chatId, cToken);

        _log.Debug($"GetCommands: {commands.Length} commands found.");

        var commandNames = !string.IsNullOrWhiteSpace(names)
            ? names.Split(',', StringSplitOptions.RemoveEmptyEntries)
            : [];

        _log.Debug($"GetCommands: {commandNames.Length} command names found.");

        var city = !string.IsNullOrWhiteSpace(cityCode)
            ? _kdmidRequestService.GetSupportedCity(cityCode!, cToken)
            : null;

        _log.Debug($"GetCommands: city code {cityCode} found.");

        _log.Debug($"GetCommands: city {city} city found.");

        var targetCommands = commands
            .Where(x => commandNames.Length == 0 || commandNames.Contains(x.Name, StringComparer.InvariantCultureIgnoreCase))
            .Where(x => cityCode == null
                || x.Parameters.TryGetValue(BotCommandParametersCityKey, out var cityStr)
                && cityStr.FromJson<City>().Code == cityCode)
            .ToArray();

        _log.Debug($"GetCommands: {targetCommands.Length} target commands found.");

        var result = targetCommands
            .Select(x =>
            {
                var city = x.Parameters[BotCommandParametersCityKey].FromJson<City>();
                var kdmidId = x.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();
                var attempts = x.Parameters.TryGetValue(BotCommandParametersAttemptsKey, out var attemptsStr)
                    ? attemptsStr.FromJson<Attempts>().Count
                    : (byte)0;

                return new CommandGetDto(x.Id.ToString(), city.Name, kdmidId.Id, kdmidId.Cd, kdmidId.Ems, attempts);
            })
            .ToArray();

        _log.Debug($"GetCommands: {result.Length} commands found.");

        return result;
    }
    #endregion

    #region Commands
    public async Task<string> CreateCommand(string chatId, CommandSetDto command, CancellationToken cToken)
    {
        var city = _kdmidRequestService.GetSupportedCity(command.CityCode, cToken);

        var kdmidId = new KdmidId()
        {
            Id = command.KdmidId,
            Cd = command.KdmidCd,
            Ems = command.KdmidEms
        };

        kdmidId.Validate();

        var parameters = new Dictionary<string, string>
        {
            { BotCommandParametersCityKey, city.ToJson() },
            { BotCommandParametersKdmidIdKey, kdmidId.ToJson() }
        };

        var botCommand = new Command(Guid.NewGuid(), KdmidBotCommandNames.CreateCommand, parameters);

        await _botResponse.Create(new(null,new(chatId)), botCommand, cToken);

        return botCommand.Id.ToString();
    }
    public async Task UpdateCommand(string chatId, string commandId, CommandSetDto command, CancellationToken cToken)
    {
        var commandIdGuid = Guid.Parse(commandId);

        var botCommand = _botCommandsStore.Get(chatId, commandIdGuid, cToken).Result;

        botCommand.Name = KdmidBotCommandNames.UpdateCommand;

        var city = _kdmidRequestService.GetSupportedCity(command.CityCode, cToken);

        var kdmidId = new KdmidId()
        {
            Id = command.KdmidId,
            Cd = command.KdmidCd,
            Ems = command.KdmidEms
        };

        kdmidId.Validate();

        if (botCommand.Parameters.ContainsKey(BotCommandParametersKdmidIdKey))
            botCommand.Parameters[BotCommandParametersKdmidIdKey] = kdmidId.ToJson();
        else
            botCommand.Parameters.Add(BotCommandParametersKdmidIdKey, kdmidId.ToJson());

        if (botCommand.Parameters.ContainsKey(BotCommandParametersCityKey))
            botCommand.Parameters[BotCommandParametersCityKey] = city.ToJson();
        else
            botCommand.Parameters.Add(BotCommandParametersCityKey, city.ToJson());

        await _botResponse.Create(new(null, new(chatId)), botCommand, cToken);
    }
    public async Task DeleteCommand(string chatId, string commandId, CancellationToken cToken)
    {
        var commandIdGuid = Guid.Parse(commandId);

        var botCommand = await _botCommandsStore.Get(chatId, commandIdGuid, cToken);

        await _botResponse.Create(new(null, new(chatId)), new Command(commandIdGuid, KdmidBotCommandNames.DeleteCommand, botCommand.Parameters), cToken);
    }
    #endregion
}
