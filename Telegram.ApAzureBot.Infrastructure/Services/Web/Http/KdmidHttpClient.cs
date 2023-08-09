using Telegram.ApAzureBot.Core;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Http;

namespace Telegram.ApAzureBot.Infrastructure.Services.Web.Http;

public sealed class KdmidHttpClient : IHttpClient
{
    private readonly HttpClient _httpClient;
    public KdmidHttpClient(IHttpClientFactory httpClientFactory) => _httpClient = httpClientFactory.CreateClient(Constants.Kdmid);

    public Task<byte[]> GetByteArrayAsync(string url, CancellationToken cToken) =>
        _httpClient.GetByteArrayAsync(url, cToken);

    public Task<string> GetStringAsync(string url, CancellationToken cToken) =>
        _httpClient.GetStringAsync(url, cToken);

    public Task<HttpResponseMessage> PostAsync(string url, StringContent content, CancellationToken cToken) =>
        _httpClient.PostAsync(url, content, cToken);
}
