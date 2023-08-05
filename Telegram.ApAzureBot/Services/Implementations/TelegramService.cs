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
    public ITelegramBotClient Client { get; }

    private readonly ILogger _logger;
    private readonly IResponseService _responseService;
    public TelegramService(ILogger<TelegramService> logger, IResponseService responseService)
    {
        _logger = logger;
        _responseService = responseService;

        var token = Environment.GetEnvironmentVariable("TelegramBotToken", EnvironmentVariableTarget.Process);

        ArgumentNullException.ThrowIfNull(token, "Telegram token was not found.");

        Client = new TelegramBotClient(token);
    }

    public Task SetWebhook(HttpRequestData request, CancellationToken cToken)
    {
        var url = request.Url.ToString().Replace(Functions.StartFunction, Functions.HandleFunction, true, CultureInfo.InvariantCulture);
        return Client.SetWebhookAsync(url, cancellationToken: cToken);
    }
    public async Task Send(HttpRequestData request, CancellationToken cToken)
    {
        var requestData = await request.ReadAsStringAsync();

        ArgumentNullException.ThrowIfNull(requestData, "Telegram request data were not found.");

        var update = JsonConvert.DeserializeObject<Update>(requestData);

        ArgumentNullException.ThrowIfNull(update, "Update is null");

        if (update.Type != UpdateType.Message || update.Message!.Type != MessageType.Text)
        {
            await Client.SendTextMessageAsync(update.Message!.Chat.Id, "The message type is not supported.", cancellationToken: cToken);
        }

        await _responseService.Process(update.Message, cToken);
    }
    public Task Listen(HttpRequestData request, CancellationToken cToken)
    {
        Client.StartReceiving(HandleUpdate, HandleError, cancellationToken: cToken);
        return Task.CompletedTask;
    }

    private Task HandleError(ITelegramBotClient client, Exception exception, CancellationToken cToken)
    {
        _logger.LogError(exception, "Error occurred while receiving a message.");
        return Task.CompletedTask;
    }
    private Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cToken) =>
        update.Type != UpdateType.Message || update.Message!.Type != MessageType.Text
            ? Client.SendTextMessageAsync(update.Message!.Chat.Id, "The message type is not supported.", cancellationToken: cToken)
            : _responseService.Process(update.Message, cToken);
}
