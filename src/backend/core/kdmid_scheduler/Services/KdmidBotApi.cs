using KdmidScheduler.Abstractions.Interfaces.Core.Services;

using Microsoft.Extensions.Logging;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Bots.Abstractions.Models.Exceptions;
using Net.Shared.Extensions.Logging;
using Net.Shared.Extensions.Serialization.Json;

namespace KdmidScheduler.Services;

public class KdmidBotApi(
    ILogger<KdmidBotApi> logger,
    IBotClient cotClient,
    IBotResponse botResponse,
    IBotCommandsStore botCommandsStore) : IKdmidBotApi
{
    private readonly ILogger _log = logger;
    private readonly IBotClient _botClient = cotClient;
    private readonly IBotResponse _botResponse = botResponse;
    private readonly IBotCommandsStore _botCommandsStore = botCommandsStore;

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
    public Task<Command> GetCommand(string chatId, string commandId, CancellationToken cToken) =>
        Guid.TryParse(commandId, out var guid) && guid != Guid.Empty
            ? _botCommandsStore.Get(chatId, guid, cToken)
            : throw new InvalidOperationException($"Command id '{commandId}' is not valid for chat '{chatId}'.");
    public async Task<Command[]> GetCommands(string chatId, string? name, CancellationToken cToken)
    {
        var commands = await _botCommandsStore.Get(chatId, cToken);

        return string.IsNullOrWhiteSpace(name)
            ? commands
            : commands.Where(x => x.Name == name).ToArray();
    }
    public async Task SetCommand(string chatId, StreamReader reader, CancellationToken cToken)
    {
        try
        {
            var data = await reader.ReadToEndAsync(cToken);

            if (string.IsNullOrWhiteSpace(data))
                throw new InvalidOperationException($"Received data is empty for chat '{chatId}'.");

            var command = data.FromJson<Command>();

            if (command.Id == Guid.Empty)
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
