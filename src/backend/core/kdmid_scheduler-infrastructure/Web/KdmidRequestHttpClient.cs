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

        var captchaImage = await response.Content.ReadAsByteArrayAsync(cToken);

        var sessionId = (response.Headers.GetValues("Set-Cookie").FirstOrDefault()?.Split(';').FirstOrDefault())
            ?? throw new InvalidOperationException($"The SessionId is not found in the response headers for the {city.Name}.");

        await _cache.SetSessionId(city, kdmidId, sessionId.Split('=')[1], cToken);

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
