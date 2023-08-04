using System.Web;

using Microsoft.Extensions.Logging;

using Telegram.ApAzureBot.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Telegram.ApAzureBot.Services.Implementations;

internal sealed class ResponseService : IResponseService
{
    ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public ResponseService(ILogger<ResponseService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task SetResponse(TelegramBotClient bot, Message message)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
            return bot.SendTextMessageAsync(message.Chat.Id, "Please, send me a text message.");

        var text = message.Text.Trim().ToLower();

        return text.StartsWith('/')
            ? ProcessCommand(bot, message.Chat.Id, text)
            : ProcessResponse(bot, message.Chat.Id, text);
    }

    private Task ProcessCommand(TelegramBotClient bot, long chatId, string command)
    {
        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
        {
            await bot.SendTextMessageAsync(message.Chat.Id, "Please, send me a valid command.");
            return;
        }

        if (!uri.Scheme.Equals(pattern, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid url", nameof(url));

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 1)
            throw new ArgumentException("Invalid url", nameof(url));

        var commandName = segments[^1].ToLower();

        ViewServiceCommandType? commandType = commandName switch
        {
            "add" => ViewServiceCommandType.Add,
            "delete" => ViewServiceCommandType.Delete,
            "launch" => ViewServiceCommandType.Launch,
            "navigate" => ViewServiceCommandType.Navigate,
            _ => null
        };

        if (!commandType.HasValue)
            return (null, null);

        var queryParams = HttpUtility.ParseQueryString(uri.Query);

        var commandParams = new Dictionary<string, object>(queryParams.Count, StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < queryParams.Count; i++)
        {
            var paramKey = queryParams.GetKey(i);

            if (paramKey is null)
                continue;

            var paramValues = queryParams.GetValues(paramKey);

            if (paramValues?.Any() == true)
            {
                if (paramValues.Length == 1)
                    commandParams.Add(paramKey, paramValues[0]);
                else
                    continue;
            }
        }

        var routeKey = string.Join("/", segments[0..^1].Prepend(uri.Host)).ToLower();

        var command = new ViewServiceCommand()
        {
            Key = routeKey,
            Name = routeKey + '/' + commandName,
            Type = commandType.Value,
            Params = commandParams
        };

        return (null, text);
    }
    private Task ProcessResponse(TelegramBotClient bot, long chatId, string response)
    {
        return bot.SendTextMessageAsync(chatId, "This way is not implemented yet.");
    }
}
