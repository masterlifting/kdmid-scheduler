using System.Net.Http.Headers;
using System.Text;

using Telegram.ApAzureBot.Core;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

namespace Telegram.ApAzureBot.Infrastructure.Services.CommandProcesses.Kdmid;

public sealed class KdmidHttpClient : IKdmidHttpClient
{
    private const string FormDataMediaType = "application/x-www-form-urlencoded";

    private readonly HttpClient _httpClient;
    public KdmidHttpClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.Kdmid);
        
        _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en,ru;q=0.5");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/117.0");
    }

    public async Task<string> GetStartPage(string url, CancellationToken cToken)
    {
        try
        {
            var page = await _httpClient.GetStringAsync(url, cToken);

            return string.IsNullOrEmpty(page)
                ? throw new Exception("Request to Kdmid is failed.")
                : page;
        }
        catch (Exception ex)
        {
            throw new Exception("Request to Kdmid is failed.", ex);
        }
    }
    public async Task<string> PostStartPageResult(Uri uri, string data, CancellationToken cToken)
    {
        try
        {
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
                ? throw new Exception("Request to Kdmid is failed.")
                : postResponseResult;

        }
        catch (Exception ex)
        {
            throw new Exception("Request to Kdmid is failed.", ex);
        }
    }
    
    public async Task PostConfirmPage(string url, string data, CancellationToken cToken)
    {
        try
        {
            var content = new StringContent(data, Encoding.UTF8, FormDataMediaType);
            var postResponse = await _httpClient.PostAsync(url, content, cToken);
        }
        catch (Exception ex)
        {
            throw new Exception("Request to Kdmid is failed.", ex);
        }
    }
    public async Task<string> GetConfirmCalendar(string url, CancellationToken cToken)
    {
        try
        {
            var page = await _httpClient.GetStringAsync(url, cToken);

            return string.IsNullOrEmpty(page)
                ? throw new Exception("Request to Kdmid is failed.")
                : page;
        }
        catch (Exception ex)
        {
            throw new Exception("Request to Kdmid is failed.", ex);
        }
    }
    
    public async Task<string> GetConfirmPageResult(string url, string data, CancellationToken cToken)
    {
        try
        {
            var content = new StringContent(data, Encoding.UTF8, FormDataMediaType);
            var postResponse = await _httpClient.PostAsync(url, content, cToken);
            var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

            return string.IsNullOrEmpty(postResponseResult)
                ? throw new Exception("Request to Kdmid is failed.")
                : postResponseResult;

        }
        catch (Exception ex)
        {
            throw new Exception("Request to Kdmid is failed.", ex);
        }
    }
}
