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
    public async Task Start(CancellationToken cToken) => 
        await _api.Listen(cToken);

    [HttpGet("listen")]
    public async Task Listen(CancellationToken cToken)
    {
        var uri = new Uri($"{Request.Scheme}://{Request.Host}/bot/receive");
        await _api.Listen(uri, cToken);
    }

    [HttpPost("receive")]
    public async Task Receive(CancellationToken cToken)
    {
        using var reader = new StreamReader(Request.Body);
        await _api.Receive(reader, cToken);
    }

    [HttpGet("chats/{chatId}/commands/{commandId}")]
    public async Task<Command> GetCommand(string chatId, string commandId, CancellationToken cToken) => 
        await _api.GetCommand(chatId, commandId, cToken);

    [HttpGet("chats/{chatId}/commands")]
    public async Task<IEnumerable<Command>> GetCommands(string chatId, CancellationToken cToken)
    {
        var filter = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
        return await _api.GetCommands(chatId, filter, cToken);
    }

    [HttpPost("chats/{chatId}/command")]
    public async Task SetCommand(string chatId, CancellationToken cToken)
    {
        using var reader = new StreamReader(Request.Body);
        await _api.SetCommand(chatId, reader, cToken);
    }
}
