using Microsoft.Azure.Functions.Worker.Http;

namespace Telegram.ApAzureBot.Services.Interfaces
{
    public interface ITelegramService
    {
        Task SetWebhook(HttpRequestData request, CancellationToken cToken);
        Task SendResponse(HttpRequestData request, CancellationToken cToken);
    }
}
