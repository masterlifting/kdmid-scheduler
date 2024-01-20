using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Models.Core.v1.BotApiDto;

using Microsoft.Extensions.Logging;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Exceptions;
using Net.Shared.Extensions.Logging;
using Net.Shared.Extensions.Serialization.Json;

using static KdmidScheduler.Constants;

namespace KdmidScheduler.Services;

public class KdmidBotApi(
    ILogger<KdmidBotApi> logger,
    IBotClient cotClient,
    IBotCommandsStore botCommandsStore,
    IKdmidRequestService kdmidRequestService
    ) : IKdmidBotApi
{
    private readonly ILogger _log = logger;
    private readonly IBotClient _botClient = cotClient;
    private readonly IBotCommandsStore _botCommandsStore = botCommandsStore;
    private readonly IKdmidRequestService _kdmidRequestService = kdmidRequestService;

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

    public async Task<CommandGetDto> GetCommand(string chatId, string commandId, CancellationToken cToken)
    {
        var commandIdGuid = Guid.Parse(commandId);
        var command = await _botCommandsStore.Get(chatId, commandIdGuid, cToken);

        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();
        var attempts = command.Parameters.TryGetValue(BotCommandParametersAttemptsKey, out var attemptsStr) 
            ? attemptsStr.FromJson<Attempts>().Count
            : (byte)0;

        return new CommandGetDto(commandId, command.Name, city.Name, kdmidId.Id, kdmidId.Cd, kdmidId.Ems, attempts);
    }
    public async Task<CommandGetDto[]> GetCommands(string chatId, string? names, string? cityCode, CancellationToken cToken)
    {
        var commands = await _botCommandsStore.Get(chatId, cToken);
        
        var commandNames = Array.Empty<string>();
        
        if(!string.IsNullOrWhiteSpace(names))
        {
            commandNames = names!.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }

        var hasCityCode = !string.IsNullOrWhiteSpace(cityCode);
        var city = hasCityCode ? _kdmidRequestService.GetSupportedCity(cityCode!, cToken) : null;

        var targetCommands = commands
            .Where(x =>
                !commandNames.Contains(x.Name, StringComparer.InvariantCultureIgnoreCase)
                || !hasCityCode
                || x.Parameters.TryGetValue(BotCommandParametersCityKey, out var cityStr)
                && cityStr.FromJson<City>().Code == cityCode)
            .ToArray();

        return targetCommands
            .Select(x =>
            {
                var city = x.Parameters[BotCommandParametersCityKey].FromJson<City>();
                var kdmidId = x.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();
                var attempts = x.Parameters.TryGetValue(BotCommandParametersAttemptsKey, out var attemptsStr) 
                    ? attemptsStr.FromJson<Attempts>().Count
                    : (byte)0;

                return new CommandGetDto(x.Id.ToString(), x.Name, city.Name, kdmidId.Id, kdmidId.Cd, kdmidId.Ems, attempts);
            })
            .ToArray();
    }
    
    public async Task CreateCommand(string chatId, CommandSetDto command, CancellationToken cToken)
    {
        try
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

            await _botCommandsStore.Create(chatId, command.Name, parameters, cToken);
        }
        catch (BotUserInvalidOperationException exception)
        {
            await _botClient.SendMessage(chatId, new(exception.Message), cToken);
            return;
        }
        catch
        {
            await _botClient.SendMessage(chatId, new(Net.Shared.Abstractions.Constants.UserErrorMessage), cToken);
            throw;
        }
    }
    public Task UpdateCommand(string chatId, string commandId, CommandSetDto command, CancellationToken cToken)
    {
        try
        {
            var commandIdGuid = Guid.Parse(commandId);

            var storedCommand = _botCommandsStore.Get(chatId, commandIdGuid, cToken).Result;

            storedCommand.Name = command.Name;

            var city = _kdmidRequestService.GetSupportedCity(command.CityCode, cToken);
            
            var kdmidId = new KdmidId()
            {
                Id = command.KdmidId,
                Cd = command.KdmidCd,
                Ems = command.KdmidEms
            };

            kdmidId.Validate();

            if(storedCommand.Parameters.ContainsKey(BotCommandParametersKdmidIdKey))
                storedCommand.Parameters[BotCommandParametersKdmidIdKey] = kdmidId.ToJson();
            else
                storedCommand.Parameters.Add(BotCommandParametersKdmidIdKey, kdmidId.ToJson());

            if(storedCommand.Parameters.ContainsKey(BotCommandParametersCityKey))
                storedCommand.Parameters[BotCommandParametersCityKey] = city.ToJson();
            else
                storedCommand.Parameters.Add(BotCommandParametersCityKey, city.ToJson());

            return _botCommandsStore.Update(chatId, commandIdGuid, storedCommand, cToken);
        }
        catch (BotUserInvalidOperationException exception)
        {
            return _botClient.SendMessage(chatId, new(exception.Message), cToken);
        }
        catch
        {
            return _botClient.SendMessage(chatId, new(Net.Shared.Abstractions.Constants.UserErrorMessage), cToken);
        }
    }
    public Task DeleteCommand(string chatId, string commandId, CancellationToken cToken)
    {
        try
        {
            var commandIdGuid = Guid.Parse(commandId);

            return _botCommandsStore.Delete(chatId, commandIdGuid, cToken);
        }
        catch (BotUserInvalidOperationException exception)
        {
            return _botClient.SendMessage(chatId, new(exception.Message), cToken);
        }
        catch
        {
            return _botClient.SendMessage(chatId, new(Net.Shared.Abstractions.Constants.UserErrorMessage), cToken);
        }
    }
}
