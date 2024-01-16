using System.Text.Json;

using KdmidScheduler.Abstractions.Interfaces.Core.Services;

using Microsoft.Extensions.Logging;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Bots.Abstractions.Models.Exceptions;
using Net.Shared.Extensions.Logging;

namespace KdmidScheduler.Services;

public class KdmidBotApi(
    ILogger<KdmidBotApi> logger,
    IBotClient cotClient,
    IBotResponse botResponse,
    IBotCommandsStore botCommandsStore) : IKdmidBotApi
{
    private readonly ILogger _logger = logger;
    private readonly IBotClient _botClient = cotClient;
    private readonly IBotResponse _botResponse = botResponse;
    private readonly IBotCommandsStore _botCommandsStore = botCommandsStore;

    public async Task Listen(CancellationToken cToken)
    {
        await _botClient.Listen(cToken);
        _logger.Info("Bot client is listening.");
    }
    public async Task Listen(Uri uri, CancellationToken cToken)
    {
        await _botClient.Listen(uri, cToken);
        _logger.Info($"Bot client is listening on {uri}.");
    }
    public async Task Receive(StreamReader reader, CancellationToken cToken)
    {
        var data = await reader.ReadToEndAsync(cToken);

        if (string.IsNullOrWhiteSpace(data))
            throw new InvalidOperationException("Received data is empty.");

        await _botClient.Receive(data, cToken);
    }
    public async Task<Command> GetCommand(string chatId, string commandId, CancellationToken cToken)
    {
        try
        {
            return Guid.TryParse(commandId, out var guid) && guid != Guid.Empty
                ? await _botCommandsStore.Get(chatId, guid, cToken)
                : throw new InvalidOperationException($"Command id '{commandId}' is not valid for chat '{chatId}'.");
        }
        catch
        {
            await _botClient.SendMessage(chatId, new(Net.Shared.Abstractions.Constants.UserErrorMessage), cToken);
            throw;
        }
    }
    public async Task<Command[]> GetCommands(string chatId, Dictionary<string, string> filter, CancellationToken cToken)
    {
        foreach (var (propName, commandName) in filter)
        {
            if(propName == "name")
            {
                var commands = await _botCommandsStore.Get(chatId, cToken);
                return commands.Where(x => x.Name == commandName).ToArray();
            }
        }

        return [];
    }
    public async Task SetCommand(string chatId, StreamReader reader, CancellationToken cToken)
    {
        try
        {
            var data = await reader.ReadToEndAsync(cToken);

            if (string.IsNullOrWhiteSpace(data))
                throw new InvalidOperationException($"Received data is empty for chat '{chatId}'.");

            var command = JsonSerializer.Deserialize<Command>(data, options: new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (command is null || command.Id == Guid.Empty)
                throw new InvalidOperationException($"Received command is empty for chat '{chatId}'.");

            await _botResponse.Create(chatId, command, cToken);
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
}
