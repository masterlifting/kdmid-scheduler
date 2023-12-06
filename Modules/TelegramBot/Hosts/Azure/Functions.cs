using System.Globalization;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Telegram.ApAzureBot.Core.Abstractions.Services;

namespace Telegram.ApAzureBot.Api;

public class Functions
{
    private const string SetReceiverFunction = "set";
    private const string ReceiveFunction = "receive";
    private const string ListenFunction = "listen";

    private readonly ITelegramClient _client;
    private readonly ITelegramCommandTaskService _service;
    public Functions(ITelegramClient telegramClient, ITelegramCommandTaskService service)
    {
        _client = telegramClient;
        _service = service
    }

    [Function(SetReceiverFunction)]
    public Task SetReceiver([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request, CancellationToken cToken)
    {
        var url = request.Url.ToString().Replace(SetReceiverFunction, ReceiveFunction, true, CultureInfo.InvariantCulture);

        return _client.SetWebhook(url, cToken);
    }

    [Function(ReceiveFunction)]
    public async Task Receive([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request, CancellationToken cToken)
    {
        var data = await request.ReadAsStringAsync();

        ArgumentNullException.ThrowIfNull(data, "Received data is empty.");

        await _client.ReceiveMessage(data, cToken);
    }

    [Function(ListenFunction)]
    public Task Listen([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request, CancellationToken cToken) =>
        _client.ListenMessages(cToken);

    [Function("fastseek")]
    public async Task RunFastSeek([TimerTrigger("15,40 6-7 * * 1-5")] TelegramTimer timer) => await _service.Process(new[]
    {
        Core.Constants.Kdmid.Cities.Budapest,
        Core.Constants.Kdmid.Cities.Belgrade
    });

    [Function("slowseek")]
    public async Task RunSlowSeek([TimerTrigger("0/30 7-14 * * 1-5")] TelegramTimer timer) => await _service.Process(Array.Empty<string>());
}
