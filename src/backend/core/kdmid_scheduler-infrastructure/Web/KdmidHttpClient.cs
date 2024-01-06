using System.Net;
using System.Net.Http.Headers;
using System.Text;
using KdmidScheduler.Abstractions.Interfaces.Services;
using KdmidScheduler.Abstractions.Models.v1;
using Microsoft.Extensions.Caching.Memory;

namespace KdmidScheduler.Infrastructure.Web;

public sealed class KdmidHttpClient(IHttpClientFactory httpClientFactory, IMemoryCache cache) : IKdmidHttpClient
{
    private readonly IMemoryCache _cache = cache;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private static string GetBaseUrl(City city) => $"https://{city.Code}.kdmid.ru/queue/";
    private static Uri GetRequestUri(City city, Identifier identifier) => new(GetBaseUrl(city) + "OrderInfo.aspx?" + identifier);

    private const string SessionIdKey = "ASP.NET_SessionId";
    private const string FormDataMediaType = "application/x-www-form-urlencoded";

    public async Task<string> GetStartPage(City city, Identifier identifier, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = GetRequestUri(city, identifier);

        var response = await httpClient.GetAsync(uri, cToken);

        if (response.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"The response status code from {uri} is {response.StatusCode}.");

        var page = await response.Content.ReadAsStringAsync(cToken);

        return string.IsNullOrEmpty(page)
            ? throw new InvalidOperationException($"The response from {uri} is empty.")
            : page;
    }
    public async Task<byte[]> GetStartPageCaptchaImage(City city, string captchaCode, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = GetBaseUrl(city) + captchaCode;

        var response = await httpClient.GetAsync(uri, cToken);

        if (response.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"The response status code from {uri} is {response.StatusCode}.");

        var captchaImage = await response.Content.ReadAsByteArrayAsync(cToken);

        var sessionId = (response.Headers.GetValues("Set-Cookie").FirstOrDefault()?.Split(';').FirstOrDefault())
            ?? throw new InvalidOperationException($"The SessionId is not found in the response headers for the {city.Name}.");

        var sessionIdData = sessionId.Split('=');

        var sessionIdValue = sessionIdData[1];

        _cache.Set(SessionIdKey, sessionIdValue);

        httpClient.DefaultRequestHeaders.Add("Cookie", $"{SessionIdKey}={sessionIdValue}");

        return captchaImage;
    }
    public async Task<string> PostApplication(City city, Identifier identifier, string content, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = GetRequestUri(city, identifier);

        var stringContent = new StringContent(content, Encoding.ASCII, new MediaTypeHeaderValue(FormDataMediaType));

        httpClient.DefaultRequestHeaders.Add("Host", uri.Host);
        httpClient.DefaultRequestHeaders.Add("Origin", uri.GetLeftPart(UriPartial.Authority));
        httpClient.DefaultRequestHeaders.Add("Referer", uri.AbsoluteUri);
        httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
        httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
        httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
        httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");

        var postResponse = await httpClient.PostAsync(uri, stringContent, cToken);

        if (postResponse.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"The response status code from {uri} is {postResponse.StatusCode}.");

        var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

        return string.IsNullOrEmpty(postResponseResult)
            ? throw new InvalidOperationException($"The response from {uri} is empty.")
            : postResponseResult;
    }
    public async Task<string> PostCalendar(City city, Identifier identifier, string content, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = GetRequestUri(city, identifier);

        var stringContent = new StringContent(content, Encoding.UTF8, FormDataMediaType);

        var postResponse = await httpClient.PostAsync(uri, stringContent, cToken);

        if (postResponse.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"The response status code from {uri} is {postResponse.StatusCode}.");

        var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

        return string.IsNullOrEmpty(postResponseResult)
            ? throw new InvalidOperationException($"The response from {uri} is empty.")
            : postResponseResult;
    }
    public async Task<string> PostConfirmation(City city, Identifier identifier, string content, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = new Uri(GetBaseUrl(city) + $"SPCalendar.aspx?bjo={identifier.Id}");

        if (!_cache.TryGetValue(SessionIdKey, out var sessionIdValue))
            throw new InvalidOperationException("The SessionId is not found in the cache.");

        var stringContent = new StringContent(content, Encoding.UTF8, FormDataMediaType);

        httpClient.DefaultRequestHeaders.Add("Cookie", $"{SessionIdKey}={sessionIdValue}");
        httpClient.DefaultRequestHeaders.Add("Host", uri.Host);
        httpClient.DefaultRequestHeaders.Add("Origin", uri.GetLeftPart(UriPartial.Authority));
        httpClient.DefaultRequestHeaders.Add("Referer", uri.AbsoluteUri);
        httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
        httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
        httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
        httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");

        var postResponse = await httpClient.PostAsync(uri, stringContent, cToken);

        if (postResponse.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"The response status code from {uri} is {postResponse.StatusCode}.");

        var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

        return string.IsNullOrEmpty(postResponseResult)
            ? throw new InvalidOperationException($"The response from {uri} is empty.")
            : postResponseResult;
    }
}
