using System.Net.Http.Headers;
using System.Text;

using Telegram.ApAzureBot.Core;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;
using Telegram.ApAzureBot.Core.Models.CommandProcesses.Kdmid;
using Telegram.ApAzureBot.Core.Services;
using Telegram.ApAzureBot.Infrastructure.Exceptions;

namespace Telegram.ApAzureBot.Infrastructure.Services.CommandProcesses.Kdmid;

public sealed class KdmidHttpClient : IKdmidHttpClient
{
    private static string GetBaseUrl(KdmidCity city) => $"https://{city.Code}.{Constants.Kdmid.Key}.ru/queue/";
    private static Uri GetRequestUri(KdmidCity city, string identifier) => new(GetBaseUrl(city) + "OrderInfo.aspx?" + identifier);

    private const string SessionIdKey = "ASP.NET_SessionId";
    private const string FormDataMediaType = "application/x-www-form-urlencoded";

    private readonly HttpClient _httpClient;
    private readonly TelegramMemoryCache _cache;
    public KdmidHttpClient(IHttpClientFactory httpClientFactory, TelegramMemoryCache cache)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.Kdmid.Key);
        _cache = cache;
    }

    public async Task<string> GetStartPage(KdmidCity city, string parameters, CancellationToken cToken)
    {
        try
        {
            var uri = GetRequestUri(city, parameters);

            var response = await _httpClient.GetAsync(uri, cToken);

            var page = await response.Content.ReadAsStringAsync(cToken);

            return string.IsNullOrEmpty(page)
                ? throw new ApAzureBotInfrastructureException($"The response from {uri} is empty.")
                : page;
        }
        catch (Exception exception)
        {
            throw new ApAzureBotInfrastructureException(exception);
        }
    }
    public async Task<byte[]> GetCaptchaImage(long chatId, KdmidCity city, string parameters, CancellationToken cToken)
    {
        try
        {
            var uri = new Uri(GetBaseUrl(city) + parameters);

            var response = await _httpClient.GetAsync(uri, cToken);

            var captchaImage = await response.Content.ReadAsByteArrayAsync(cToken);

            var sessionId = (response.Headers.GetValues("Set-Cookie").FirstOrDefault()?.Split(';').FirstOrDefault())
                ?? throw new ApAzureBotInfrastructureException("The SessionId is not found in the response headers.");

            var sessionIdData = sessionId.Split('=');

            var sessionIdKey = SessionIdKey;
            var sessionIdValue = sessionIdData[1];

            _cache.AddOrUpdate(chatId, sessionIdKey, sessionIdValue);
            _httpClient.DefaultRequestHeaders.Add("Cookie", $"{sessionIdKey}={sessionIdValue}");

            return captchaImage;

        }
        catch (Exception exception)
        {
            throw new ApAzureBotInfrastructureException(exception);
        }
    }
    public async Task<string> PostStartPageResult(long chatId, KdmidCity city, string parameters, string data, CancellationToken cToken)
    {
        try
        {
            var uri = GetRequestUri(city, parameters);

            var content = new StringContent(data, Encoding.ASCII, new MediaTypeHeaderValue(FormDataMediaType));

            _httpClient.DefaultRequestHeaders.Add("Host", uri.Host);
            _httpClient.DefaultRequestHeaders.Add("Origin", uri.GetLeftPart(UriPartial.Authority));
            _httpClient.DefaultRequestHeaders.Add("Referer", uri.AbsoluteUri);
            _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");

            var postResponse = await _httpClient.PostAsync(uri, content, cToken);

            var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

            return string.IsNullOrEmpty(postResponseResult)
                ? throw new ApAzureBotInfrastructureException($"The response from {uri} is empty.")
                : postResponseResult;
        }
        catch (Exception exception)
        {
            throw new ApAzureBotInfrastructureException(exception);
        }
    }

    public async Task<string> PostConfirmPageResult(KdmidCity city, string parameters, string data, CancellationToken cToken)
    {
        try
        {
            var uri = GetRequestUri(city, parameters);

            var content = new StringContent(data, Encoding.UTF8, FormDataMediaType);

            var postResponse = await _httpClient.PostAsync(uri, content, cToken);

            var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

            return string.IsNullOrEmpty(postResponseResult)
                ? throw new ApAzureBotInfrastructureException($"The response from {uri} is empty.")
                : postResponseResult;
        }
        catch (Exception exception)
        {
            throw new ApAzureBotInfrastructureException(exception);
        }
    }

    public async Task<string> PostConfirmPageResult(long chatId, KdmidCity city, string parameters, string data, CancellationToken cToken)
    {
        try
        {
            var uri = GetRequestUri(city, parameters);

            if (!_cache.TryGetValue(chatId, SessionIdKey, out var sessionIdValue))
                throw new ApAzureBotInfrastructureException("The SessionId is not found in the cache.");

            var content = new StringContent(data, Encoding.UTF8, FormDataMediaType);

            _httpClient.DefaultRequestHeaders.Add("Cookie", $"{SessionIdKey}={sessionIdValue}");
            _httpClient.DefaultRequestHeaders.Add("Host", uri.Host);
            _httpClient.DefaultRequestHeaders.Add("Origin", uri.GetLeftPart(UriPartial.Authority));
            _httpClient.DefaultRequestHeaders.Add("Referer", uri.AbsoluteUri);
            _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");

            var postResponse = await _httpClient.PostAsync(uri, content, cToken);

            var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

            return string.IsNullOrEmpty(postResponseResult)
                ? throw new ApAzureBotInfrastructureException($"The response from {uri} is empty.")
                : postResponseResult;

        }
        catch (Exception exception)
        {
            throw new ApAzureBotInfrastructureException(exception);
        }
    }
}
