using System.Net;
using System.Net.Http.Headers;
using System.Text;

using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Services;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;

namespace KdmidScheduler.Infrastructure.Web;

public sealed class KdmidHttpClient(
    IHttpClientFactory httpClientFactory,
    IPersistenceReaderRepository<KdmidRequestCache> readerCache,
    IPersistenceWriterRepository<KdmidRequestCache> writerCache) : IKdmidHttpClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IPersistenceReaderRepository<KdmidRequestCache> _readerCache = readerCache;
    private readonly IPersistenceWriterRepository<KdmidRequestCache> _writerCache = writerCache;

    private static string GetBaseUrl(City city) => $"https://{city.Code}.kdmid.ru/queue/";
    private static Uri GetRequestUri(City city, KdmidId kdmidId) => new(GetBaseUrl(city) + "OrderInfo.aspx?" + kdmidId);

    private const string SessionIdKey = "ASP.NET_SessionId";
    private const string FormDataMediaType = "application/x-www-form-urlencoded";

    public async Task<string> GetStartPage(City city, KdmidId kdmidId, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = GetRequestUri(city, kdmidId);

        var response = await httpClient.GetAsync(uri, cToken);

        if (response.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"The response status code from {uri} is {response.StatusCode}.");

        var page = await response.Content.ReadAsStringAsync(cToken);

        return string.IsNullOrEmpty(page)
            ? throw new InvalidOperationException($"The response from {uri} is empty.")
            : page;
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

        var sessionIdData = sessionId.Split('=');

        var sessionIdValue = sessionIdData[1];

        var isCacheExist = await _readerCache.IsExists<KdmidRequestCache>(new()
        {
            Filter = x => x.City.Code == city.Code && x.KdmidId.Id == kdmidId.Id
        }, cToken);

        if (isCacheExist)
        {
            await _writerCache.Update<KdmidRequestCache>(new(x => x.Headers = new Dictionary<string, string> { { SessionIdKey, sessionIdValue } })
            {
                QueryOptions = new()
                {
                    Filter = x => x.City.Code == city.Code && x.KdmidId.Id == kdmidId.Id
                }
            }, cToken);
        }
        else
        {
            await _writerCache.CreateOne<KdmidRequestCache>(new()
            {
                City = city,
                KdmidId = kdmidId,
                Headers = new Dictionary<string, string> { { SessionIdKey, sessionIdValue } }
            }, cToken);
        };

        return captchaImage;
    }
    public async Task<string> PostApplication(City city, KdmidId kdmidId, string content, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = GetRequestUri(city, kdmidId);

        var stringContent = new StringContent(content, Encoding.ASCII, new MediaTypeHeaderValue(FormDataMediaType));

        var cache = await _readerCache.FindSingle<KdmidRequestCache>(new()
        {
            Filter = x => x.City.Code == city.Code && x.KdmidId.Id == kdmidId.Id
        }, cToken);

        if (cache is null || !cache.Headers.TryGetValue(SessionIdKey, out string? sessionIdValue))
            throw new InvalidOperationException($"SessionId for '{city.Name}' with '{kdmidId.Id}' was not found.");

        var kdmidHeaders = new Dictionary<string, string>
        {
            {"Cookie", $"{SessionIdKey}={sessionIdValue}"},
            {"Host", uri.Host},
            {"Origin", uri.GetLeftPart(UriPartial.Authority)},
            {"Referer", uri.AbsoluteUri},
            {"Upgrade-Insecure-Requests", "1"},
            {"Sec-Fetch-Dest", "document"},
            {"Sec-Fetch-Mode", "navigate"},
            {"Sec-Fetch-Site", "same-origin"},
            {"Sec-Fetch-User", "?1"}
        };

        await _writerCache.Update<KdmidRequestCache>(new(x => x.Headers = kdmidHeaders)
        {
            QueryOptions = new()
            {
                Filter = x => x.City.Code == city.Code && x.KdmidId.Id == kdmidId.Id
            }
        }, cToken);

        foreach (var (key, value) in kdmidHeaders)
            httpClient.DefaultRequestHeaders.Add(key, value);

        var postResponse = await httpClient.PostAsync(uri, stringContent, cToken);

        if (postResponse.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"The response status code from {uri} is {postResponse.StatusCode}.");

        var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

        return string.IsNullOrEmpty(postResponseResult)
            ? throw new InvalidOperationException($"The response from {uri} is empty.")
            : postResponseResult;
    }
    public async Task<string> PostCalendar(City city, KdmidId kdmidId, string content, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = GetRequestUri(city, kdmidId);

        var stringContent = new StringContent(content, Encoding.UTF8, FormDataMediaType);

        var cache = await _readerCache.FindSingle<KdmidRequestCache>(new()
        {
            Filter = x => x.City.Code == city.Code && x.KdmidId.Id == kdmidId.Id
        }, cToken);

        if (cache is null || cache.Headers.Count == 0)
            throw new InvalidOperationException($"Cache for '{city.Name}' with '{kdmidId.Id}' was not found.");

        foreach (var (key, value) in cache.Headers)
            httpClient.DefaultRequestHeaders.Add(key, value);

        var postResponse = await httpClient.PostAsync(uri, stringContent, cToken);

        if (postResponse.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"The response status code from {uri} is {postResponse.StatusCode}.");

        var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

        return string.IsNullOrEmpty(postResponseResult)
            ? throw new InvalidOperationException($"The response from {uri} is empty.")
            : postResponseResult;
    }
    public async Task<string> PostConfirmation(City city, KdmidId kdmidId, string content, CancellationToken cToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.Kdmid);

        var uri = new Uri(GetBaseUrl(city) + $"SPCalendar.aspx?bjo={kdmidId.Id}");

        var stringContent = new StringContent(content, Encoding.UTF8, FormDataMediaType);

        var cache = await _readerCache.FindSingle<KdmidRequestCache>(new()
        {
            Filter = x => x.City.Code == city.Code && x.KdmidId.Id == kdmidId.Id
        }, cToken);

        if (cache is null || cache.Headers.Count == 0)
            throw new InvalidOperationException($"Cache for '{city.Name}' with '{kdmidId.Id}' was not found.");

        foreach (var (key, value) in cache.Headers)
            httpClient.DefaultRequestHeaders.Add(key, value);

        var postResponse = await httpClient.PostAsync(uri, stringContent, cToken);

        if (postResponse.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"The response status code from {uri} is {postResponse.StatusCode}.");

        var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

        return string.IsNullOrEmpty(postResponseResult)
            ? throw new InvalidOperationException($"The response from {uri} is empty.")
            : postResponseResult;
    }
}
