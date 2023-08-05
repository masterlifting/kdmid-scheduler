using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

using Telegram.ApAzureBot.Services.Interfaces;

namespace Telegram.ApAzureBot;

public class Functions
{
    internal const string StartFunction = "setup";
    internal const string ListenFunction = "listen";
    internal const string HandleFunction = "handle";

    private readonly ITelegramService _telegramService;

    public Functions(ITelegramService telegramService) => _telegramService = telegramService;

    [Function(StartFunction)]
    public Task Start([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request, CancellationToken cToken) => 
        _telegramService.SetWebhook(request, cToken);

    [Function(ListenFunction)]
    public Task Listen([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request, CancellationToken cToken) =>
        _telegramService.Listen(request, cToken);

    [Function(HandleFunction)]
    public Task Handle([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request, CancellationToken cToken) =>
        _telegramService.Send(request, cToken);
}
