using System.Net;
using System.Net.Http.Headers;
using System.Text;

using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Repositories;
using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Web;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

namespace KdmidScheduler.Infrastructure.Web;

public sealed class KdmidRequestHttpClient(
    IHttpClientFactory httpClientFactory,
    IKdmidRequestHttpClientCache cache) : IKdmidRequestHttpClient
{
    private readonly IKdmidRequestHttpClientCache _cache = cache;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private const string FormDataMediaType = "application/x-www-form-urlencoded";

    public async Task<string> GetStartPage(City city, KdmidId kdmidId, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = GetRequestUri(city, kdmidId);

        var response = await httpClient.GetAsync(uri, cToken);

        return await GetContent(response, cToken);
    }
    public async Task<byte[]> GetStartPageCaptchaImage(City city, KdmidId kdmidId, string captchaCode, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = GetBaseUrl(city) + captchaCode;

        var response = await httpClient.GetAsync(uri, cToken);

        if (response.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"The response status code from {uri} is {response.StatusCode}.");

        ushort keepAlive = 60;
        
        if (response.Headers.TryGetValues("Keep-Alive", out var keepAliveHeaders))
        {
            var keepAliveTimeout = keepAliveHeaders.FirstOrDefault(x => x.Contains("timeout"));
            
            if (keepAliveTimeout is not null)
            {
                keepAliveTimeout = keepAliveTimeout.Split('=')[1];

                if(ushort.TryParse(keepAliveTimeout, out var keepAliveTimeoutValue))
                {
                    keepAlive = keepAliveTimeoutValue;
                }
            }
        }

        string sessionId;

        if (!response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
            sessionId = await _cache.GetSessionId(city, kdmidId, cToken);

        sessionId = cookieHeaders!.FirstOrDefault(x => x.Contains("ASP.NET_SessionId"))
            ?? throw new InvalidOperationException($"SessionId is not found in the response from {uri}.");

        sessionId = sessionId.Split(';')[0].Split('=')[1];

        await _cache.SetSessionId(city, kdmidId, sessionId, keepAlive, cToken);

        var captchaImage = await response.Content.ReadAsByteArrayAsync(cToken);

        return captchaImage;
    }
    public async Task<string> PostApplication(City city, KdmidId kdmidId, string content, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = GetRequestUri(city, kdmidId);

        var sessionId = await _cache.GetSessionId(city, kdmidId, cToken);

        var headers = new Dictionary<string, string>
        {
            {"Cookie", $"ASP.NET_SessionId={sessionId}"},
            {"Host", uri.Host},
            {"Origin", uri.GetLeftPart(UriPartial.Authority)},
            {"Referer", uri.AbsoluteUri},
            {"Upgrade-Insecure-Requests", "1"},
            {"Sec-Fetch-Dest", "document"},
            {"Sec-Fetch-Mode", "navigate"},
            {"Sec-Fetch-Site", "same-origin"},
            {"Sec-Fetch-User", "?1"}
        };

        await _cache.SetHeaders(city, kdmidId, headers, cToken);

        foreach (var (key, value) in headers)
            httpClient.DefaultRequestHeaders.Add(key, value);

        var stringContent = new StringContent(content, Encoding.ASCII, new MediaTypeHeaderValue(FormDataMediaType));

        var postResponse = await httpClient.PostAsync(uri, stringContent, cToken);

        return await GetContent(postResponse, cToken);
    }
    public async Task<string> PostCalendar(City city, KdmidId kdmidId, string content, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = GetRequestUri(city, kdmidId);

        var headers = await _cache.GetHeaders(city, kdmidId, cToken);

        foreach (var (key, value) in headers)
            httpClient.DefaultRequestHeaders.Add(key, value);

        var stringContent = new StringContent(content, Encoding.UTF8, FormDataMediaType);

        var postResponse = await httpClient.PostAsync(uri, stringContent, cToken);

        return await GetContent(postResponse, cToken);
    }
    public async Task<string> PostConfirmation(City city, KdmidId kdmidId, string content, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = new Uri(GetBaseUrl(city) + $"SPCalendar.aspx?bjo={kdmidId.Id}");

        var headers = await _cache.GetHeaders(city, kdmidId, cToken);

        foreach (var (key, value) in headers)
            httpClient.DefaultRequestHeaders.Add(key, value);

        var stringContent = new StringContent(content, Encoding.UTF8, FormDataMediaType);

        var postResponse = await httpClient.PostAsync(uri, stringContent, cToken);

        return await GetContent(postResponse, cToken);
    }

    private static string GetBaseUrl(City city) => $"https://{city.Code}.kdmid.ru/queue/";
    private static Uri GetRequestUri(City city, KdmidId kdmidId) => new(GetBaseUrl(city) + "OrderInfo.aspx?" + kdmidId);
    private static async Task<string> GetContent(HttpResponseMessage response, CancellationToken cToken)
    {
        if (response.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"The response status code from {response.RequestMessage?.RequestUri} is {response.StatusCode}.");

        var content = await response.Content.ReadAsStringAsync(cToken);

        return string.IsNullOrEmpty(content)
            ? throw new InvalidOperationException($"The response from {response.RequestMessage?.RequestUri} is empty.")
            : content;
    }
}
