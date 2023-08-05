using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

using Telegram.ApAzureBot.Services.Interfaces;

namespace Telegram.ApAzureBot;

public class Functions
{
    internal const string StartFunction = "setup";
    internal const string HandleFunction = "handle";

    private readonly ITelegramService _telegramService;
    private readonly IResponseService _webService;

    public Functions(ITelegramService telegramService, IResponseService webService)
    {
        _telegramService = telegramService;
        _webService = webService;
    }

    [Function(StartFunction)]
    public async Task Start([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request, CancellationToken cToken)
    {
        await _webService.SetResponse("/midrf/schedule?id=54437&cd=9CFA9945");
        
        //await _telegramService.SetWebhook(request, cToken);
    }

    [Function(HandleFunction)]
    public Task Handle([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request, CancellationToken cToken) =>
        _telegramService.SendResponse(request, cToken);
}
