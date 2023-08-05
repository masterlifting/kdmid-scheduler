using Microsoft.Azure.Functions.Worker.Http;

using Newtonsoft.Json;

using System.Globalization;

using Telegram.ApAzureBot.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;

namespace Telegram.ApAzureBot.Services.Implementations;

internal sealed class TelegramService : ITelegramService
{
    public TelegramBotClient Bot { get; }
    
    private readonly ILogger _logger;
    private readonly IResponseService _responseService;
    public TelegramService(ILogger<TelegramService> logger, IResponseService responseService)
    {
        _logger = logger;
        _responseService = responseService;

        var token = Environment.GetEnvironmentVariable("TelegramBotToken", EnvironmentVariableTarget.Process);

        ArgumentNullException.ThrowIfNull(token, "Telegram token was not found.");

        Bot = new TelegramBotClient(token);
    }

    public Task SetWebhook(HttpRequestData request, CancellationToken cToken)
    {
        var url = request.Url.ToString().Replace(Functions.StartFunction, Functions.HandleFunction, true, CultureInfo.InvariantCulture);
        return Bot.SetWebhookAsync(url, cancellationToken: cToken);
    }
    public async Task SendResponse(HttpRequestData request, CancellationToken cToken)
    {
        var requestData = await request.ReadAsStringAsync();

        ArgumentNullException.ThrowIfNull(requestData, "Telegram request data were not found.");

        var update = JsonConvert.DeserializeObject<Update>(requestData);

        ArgumentNullException.ThrowIfNull(update, "Update is null");

        if (update.Type != UpdateType.Message || update.Message!.Type != MessageType.Text)
        {
            _logger.LogWarning("Telegram Update type is not supported: {0}", update.Type);
            return;
        }

        await _responseService.Process(update.Message, cToken);
    }
}
