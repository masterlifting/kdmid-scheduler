using Microsoft.Azure.Functions.Worker.Http;

using Telegram.Bot;

namespace Telegram.ApAzureBot.Services.Interfaces;

public interface ITelegramService
{
    ITelegramBotClient Client { get; }
    Task SetWebhook(HttpRequestData request, CancellationToken cToken);
    Task Listen(HttpRequestData request, CancellationToken cToken);
    Task Send(HttpRequestData request, CancellationToken cToken);
}
