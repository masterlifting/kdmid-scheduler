using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Core.v1.KdmidWebApi;

using Microsoft.AspNetCore.Mvc;

namespace KdmidScheduler.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public sealed class BotController(IKdmidBotApi api) : ControllerBase
{
    private readonly IKdmidBotApi _api = api;

    #region Client
    [HttpGet("start")]
    public Task Start(CancellationToken cToken) =>
        _api.Listen(cToken);

    [HttpGet("listen")]
    public Task Listen(CancellationToken cToken) =>
        _api.Listen(new($"{Request.Scheme}s://{Request.Host}/bot/receive"), cToken);

    [HttpPost("receive")]
    public Task Receive(CancellationToken cToken)
    {
        using var reader = new StreamReader(Request.Body);
        return _api.Receive(reader, cToken);
    }
    #endregion

    #region Queries
    [HttpGet("chats/{chatId}/commands/{commandId}")]
    public Task<CommandGetDto> GetCommand(string chatId, string commandId, CancellationToken cToken) =>
        _api.GetCommand(chatId, commandId, cToken);
    
    [HttpGet("chats/{chatId}/commands")]
    public Task<CommandGetDto[]> GetCommands(string chatId, string? names, string? cityCode, CancellationToken cToken) =>
        _api.GetCommands(chatId, names, cityCode, cToken);

    [HttpGet("cities")]
    public Task<CityGetDto[]> GetCities(CancellationToken cToken) =>
        _api.GetCities(cToken);
    #endregion

    #region Commands
    [HttpPost("chats/{chatId}/commands")]
    public Task<string> CreateCommand([FromBody] CommandSetDto command, string chatId, CancellationToken cToken) =>
        _api.CreateCommand(chatId, command, cToken);

    [HttpPut("chats/{chatId}/commands/{commandId}")]
    public Task UpdateCommand([FromBody] CommandSetDto command, string chatId, string commandId, CancellationToken cToken) =>
        _api.UpdateCommand(chatId, commandId, command, cToken);

    [HttpDelete("chats/{chatId}/commands/{commandId}")]
    public Task DeleteCommand(string chatId, string commandId, CancellationToken cToken) =>
        _api.DeleteCommand(chatId, commandId, cToken);
    #endregion
}
