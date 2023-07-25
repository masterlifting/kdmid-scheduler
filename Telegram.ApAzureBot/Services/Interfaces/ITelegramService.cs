using Microsoft.Azure.Functions.Worker.Http;

namespace Telegram.ApAzureBot.Services.Interfaces
{
    public interface ITelegramService
    {
        Task SetWebHook(HttpRequestData request, CancellationToken cToken);
        Task SendMessage(HttpRequestData request, CancellationToken cToken);
    }
}
