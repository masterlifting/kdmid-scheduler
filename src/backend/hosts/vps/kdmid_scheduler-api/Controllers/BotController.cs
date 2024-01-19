using KdmidScheduler.Abstractions.Interfaces.Core.Services;

using Microsoft.AspNetCore.Mvc;

using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public sealed class BotController(IKdmidBotApi api) : ControllerBase
{
    private readonly IKdmidBotApi _api = api;

    [HttpGet("start")]
    public Task Start(CancellationToken cToken) =>
        _api.Listen(cToken);

    [HttpGet("listen")]
    public Task Listen(CancellationToken cToken) =>
        _api.Listen(new($"{Request.Scheme}://{Request.Host}/bot/receive"), cToken);

    [HttpPost("receive")]
    public Task Receive(CancellationToken cToken)
    {
        using var reader = new StreamReader(Request.Body);
        return _api.Receive(reader, cToken);
    }


    [HttpGet("chats/{chatId}/commands/{commandId}")]
    public async Task<Command> GetCommand(string chatId, string commandId, CancellationToken cToken) =>
        await _api.GetCommand(chatId, commandId, cToken);

    [HttpGet("chats/{chatId}/commands")]
    public Task<Command[]> GetCommands(string chatId, string? name, CancellationToken cToken) =>
        _api.GetCommands(chatId, name, cToken);

    [HttpPost("chats/{chatId}/commands")]
    public Task SetCommand(string chatId, CancellationToken cToken)
    {
        using var reader = new StreamReader(Request.Body);
        return Task.Run(() => _api.SetCommand(chatId, reader, cToken), cToken);
    }
}
