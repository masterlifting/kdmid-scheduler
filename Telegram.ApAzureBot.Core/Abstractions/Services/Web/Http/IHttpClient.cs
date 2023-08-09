namespace Telegram.ApAzureBot.Core.Abstractions.Services.Web.Http;

public interface IHttpClient
{
    Task<byte[]> GetByteArrayAsync(string url, CancellationToken cToken);
    Task<string> GetStringAsync(string url, CancellationToken cToken);
    Task<HttpResponseMessage> PostAsync(string url, StringContent content, CancellationToken cToken);
}
