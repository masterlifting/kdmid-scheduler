using Microsoft.AspNetCore.Mvc.Filters;
using Net.Shared.Extensions.Logging;
using Net.Shared.Bots.Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Net.Shared.Abstractions.Models.Exceptions;

using static Net.Shared.Abstractions.Constants;

namespace KdmidScheduler.Api;

public sealed class BotExceptionFilter(
    ILogger<BotExceptionFilter> logger,
    IBotClient botClient) : IAsyncExceptionFilter
{
    private readonly ILogger _log = logger;
    private readonly IBotClient _botClient = botClient;

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        _log.ErrorFull(context.Exception);

        switch (context.Exception)
        {
            case UserInvalidOperationException exception:
                context.Result = new ObjectResult(new { message = exception.Message })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
                break;
            default:
                await _botClient.SendMessage(_botClient.AdminId, new(context.Exception.Message), CancellationToken.None);

                context.Result = new ObjectResult(new { message = UserErrorMessage })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                break;
        }

        context.ExceptionHandled = true;
    }
}
