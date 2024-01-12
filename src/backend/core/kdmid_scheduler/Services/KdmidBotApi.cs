using System.Text.Json;

using KdmidScheduler.Abstractions.Interfaces.Core.Services;

using Microsoft.Extensions.Logging;
using Net.Shared.Extensions.Logging;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Services;

public class KdmidBotApi(
    ILogger<KdmidBotApi> logger,
    IBotClient client,
    IBotResponse response,
    IBotCommandsStore commandsStore) : IKdmidBotApi
{
    private readonly ILogger _logger = logger;
    private readonly IBotClient _client = client;
    private readonly IBotResponse _response = response;
    private readonly IBotCommandsStore _commandsStore = commandsStore;


    public async Task Listen(CancellationToken cToken)
    {
        await _client.Listen(cToken);
        _logger.Info("Bot client is listening.");
    }
    public async Task Listen(Uri uri, CancellationToken cToken)
    {
        await _client.Listen(uri, cToken);
        _logger.Info($"Bot client is listening on {uri}.");
    }
    public async Task Receive(StreamReader reader, CancellationToken cToken)
    {
        var data = await reader.ReadToEndAsync(cToken);

        ArgumentNullException.ThrowIfNull(data, "Received data is empty.");

        await _client.Receive(data, cToken);
    }
    public Task<BotCommand> GetCommand(string chatId, string commandId, CancellationToken cToken)
    {
        if(Guid.TryParse(commandId, out var guid) && guid != Guid.Empty)
            return _commandsStore.Get(chatId, guid, cToken);

        throw new ArgumentException("Command id is not valid.", nameof(commandId));
    }
    public async Task UpdateCommand(string chatId, StreamReader reader, CancellationToken cToken)
    {
        var data = await reader.ReadToEndAsync(cToken);

        ArgumentNullException.ThrowIfNull(data, "Received data is empty.");

        var command = JsonSerializer.Deserialize<BotCommand>(data, options: new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (command is null || command.Id == Guid.Empty)
            throw new ArgumentNullException(nameof(command), "Received command is empty.");

        await _commandsStore.Update(chatId, command.Id, command, cToken);

        await _response.Create(chatId, command, cToken);
    }
}
