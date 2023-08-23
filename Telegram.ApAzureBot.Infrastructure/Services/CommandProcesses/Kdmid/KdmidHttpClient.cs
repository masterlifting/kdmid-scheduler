using System.Net.Http.Headers;
using System.Text;

using Telegram.ApAzureBot.Core;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;
using Telegram.ApAzureBot.Core.Services;
using Telegram.ApAzureBot.Infrastructure.Exceptions;

namespace Telegram.ApAzureBot.Infrastructure.Services.CommandProcesses.Kdmid;

public sealed class KdmidHttpClient : IKdmidHttpClient
{
    private const string SessionIdKey = "ASP.NET_SessionId";
    private const string FormDataMediaType = "application/x-www-form-urlencoded";

    private readonly HttpClient _httpClient;
    private readonly TelegramMemoryCache _cache;
    public KdmidHttpClient(IHttpClientFactory httpClientFactory, TelegramMemoryCache cache)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.Kdmid);
        _cache = cache;
    }

    public async Task<string> GetStartPage(string url, CancellationToken cToken)
    {
        try
        {
            var page = await _httpClient.GetStringAsync(url, cToken);

            return string.IsNullOrEmpty(page)
                ? throw new ApAzureBotInfrastructureException("Request to Kdmid is failed.")
                : page;
        }
        catch (Exception exception)
        {
            throw new ApAzureBotInfrastructureException(exception);
        }
    }
    public async Task<byte[]> GetCaptchaImage(long chatId, string url, CancellationToken cToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, cToken);
            var captchaImage = await response.Content.ReadAsByteArrayAsync(cToken);
            
            var sessionId = response.Headers.GetValues("Set-Cookie").FirstOrDefault()?.Split(';').FirstOrDefault();
            
            if(sessionId is null)
                throw new ApAzureBotInfrastructureException("Request to Kdmid is failed.");

            var sessionIdData = sessionId.Split('=');

            var sessionIdKey = SessionIdKey;
            var sessionIdValue = sessionIdData[1];

            _cache.AddOrUpdate(chatId, sessionIdKey, sessionIdValue);

            return captchaImage;

        }
        catch (Exception exception)
        {
            throw new ApAzureBotInfrastructureException(exception);
        }
    }
    public async Task<string> PostStartPageResult(long chatId, Uri uri, string data, CancellationToken cToken)
    {
        try
        {
            if(!_cache.TryGetValue(chatId, SessionIdKey, out var sessionIdValue))
                throw new ApAzureBotInfrastructureException("Request to Kdmid is failed.");

            var content = new StringContent(data, Encoding.ASCII, new MediaTypeHeaderValue(FormDataMediaType));

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
                ? throw new ApAzureBotInfrastructureException("Request to Kdmid is failed.")
                : postResponseResult;
        }
        catch (Exception exception)
        {
            throw new ApAzureBotInfrastructureException(exception);
        }
    }
    
    public async Task PostConfirmPage(long chatId, string url, string data, CancellationToken cToken)
    {
        try
        {
            if (!_cache.TryGetValue(chatId, SessionIdKey, out var sessionIdValue))
                throw new ApAzureBotInfrastructureException("Request to Kdmid is failed.");

            var content = new StringContent(data, Encoding.UTF8, FormDataMediaType);

            _httpClient.DefaultRequestHeaders.Add("Cookie", $"{SessionIdKey}={sessionIdValue}");

            var postResponse = await _httpClient.PostAsync(url, content, cToken);
        }
        catch (Exception exception)
        {
            throw new ApAzureBotInfrastructureException(exception);
        }
    }
    public async Task<string> GetConfirmCalendar(long chatId, string url, CancellationToken cToken)
    {
        try
        {
            if (!_cache.TryGetValue(chatId, SessionIdKey, out var sessionIdValue))
                throw new ApAzureBotInfrastructureException("Request to Kdmid is failed.");

            _httpClient.DefaultRequestHeaders.Add("Cookie", $"{SessionIdKey}={sessionIdValue}");

            var response = await _httpClient.GetAsync(url, cToken);
            
            var page = await response.Content.ReadAsStringAsync(cToken);

            return string.IsNullOrEmpty(page)
                ? throw new ApAzureBotInfrastructureException("Request to Kdmid is failed.")
                : page;
        }
        catch (Exception exception)
        {
            throw new ApAzureBotInfrastructureException(exception);
        }
    }
    
    public async Task<string> GetConfirmPageResult(long chatId, string url, string data, CancellationToken cToken)
    {
        try
        {
            var content = new StringContent(data, Encoding.UTF8, FormDataMediaType);
            var postResponse = await _httpClient.PostAsync(url, content, cToken);
            var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

            return string.IsNullOrEmpty(postResponseResult)
                ? throw new ApAzureBotInfrastructureException("Request to Kdmid is failed.")
                : postResponseResult;

        }
        catch (Exception exception)
        {
            throw new ApAzureBotInfrastructureException(exception);
        }
    }
}
