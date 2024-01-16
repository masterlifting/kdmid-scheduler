using Microsoft.AspNetCore.Mvc.Filters;
using Net.Shared.Extensions.Logging;
using Net.Shared.Bots.Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KdmidScheduler.Api;

public sealed class BotExceptionFilter(
    ILogger<BotExceptionFilter> logger,
    IBotClient botClient) : IAsyncActionFilter
{
    private readonly ILogger _log = logger;
    private readonly IBotClient _botClient = botClient;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            var resultContext = await next();

            if (resultContext.Exception != null)
            {
                _log.ErrorCompact(resultContext.Exception);

                await _botClient.SendMessage(_botClient.AdminId, new(resultContext.Exception.Message), CancellationToken.None);

                var result = new ObjectResult(new { message = "An error occurred." })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };

                resultContext.Result = result;
                resultContext.ExceptionHandled = true;
            }
        }
        catch (Exception exception)
        {
            _log.ErrorCompact(exception);

            await _botClient.SendMessage(_botClient.AdminId, new(exception.Message), CancellationToken.None);
        }
    }
}
