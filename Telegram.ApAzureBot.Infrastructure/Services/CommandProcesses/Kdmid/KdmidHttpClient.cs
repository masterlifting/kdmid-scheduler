using System.Text;

using Telegram.ApAzureBot.Core;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

namespace Telegram.ApAzureBot.Infrastructure.Services.CommandProcesses.Kdmid;

public sealed class KdmidHttpClient : IKdmidHttpClient
{
    private const string FormDataMediaType = "application/x-www-form-urlencoded";

    private readonly HttpClient _httpClient;
    public KdmidHttpClient(IHttpClientFactory httpClientFactory) => _httpClient = httpClientFactory.CreateClient(Constants.Kdmid);

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
    
    public async Task<string> GetConfirmPage(string url, string data, CancellationToken cToken)
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
