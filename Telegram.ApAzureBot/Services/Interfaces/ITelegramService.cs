using Microsoft.Azure.Functions.Worker.Http;

using Telegram.Bot;

namespace Telegram.ApAzureBot.Services.Interfaces;

public interface ITelegramService
{
    TelegramBotClient Bot { get; }
    Task SetWebhook(HttpRequestData request, CancellationToken cToken);
    Task SendResponse(HttpRequestData request, CancellationToken cToken);
}
