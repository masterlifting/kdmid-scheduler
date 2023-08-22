using System.Text;

using Azure.Core;

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
        
        _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en,ru;q=0.9");
        _httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
        _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36 Edg/115.0.1901.203");
    }

    public async Task<string> GetStartPage(string url, CancellationToken cToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            var response = await _httpClient.SendAsync(request, cToken);

            var page = await response.Content.ReadAsStringAsync(cToken);

            return string.IsNullOrEmpty(page)
                ? throw new Exception("Request to Kdmid is failed.")
                : page;
        }
        catch (Exception ex)
        {
            throw new Exception("Request to Kdmid is failed.", ex);
        }
    }
    public async Task<string> GetStartPageResult(string url, string data, CancellationToken cToken)
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
