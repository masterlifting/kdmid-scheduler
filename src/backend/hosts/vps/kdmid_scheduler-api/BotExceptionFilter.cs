using Microsoft.AspNetCore.Mvc.Filters;
using Net.Shared.Extensions.Logging;
using Net.Shared.Bots.Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Net.Shared.Bots.Abstractions.Models.Settings;
using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Api;

public sealed class BotExceptionFilter(
    IOptions<TelegramBotConnectionSettings> options,
    ILogger<BotExceptionFilter> logger, 
    IBotClient botClient) : IAsyncActionFilter
{
    private readonly TelegramBotConnectionSettings _botConnectionSettings = options.Value;
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

                var messageArgs = new MessageEventArgs(_botConnectionSettings.AdminChatId, new(resultContext.Exception.Message));
                await _botClient.SendMessage(messageArgs, CancellationToken.None);

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

            var messageArgs = new MessageEventArgs(_botConnectionSettings.AdminChatId, new(exception.Message));
            await _botClient.SendMessage(messageArgs, CancellationToken.None);
        }
    }
}
