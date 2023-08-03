using Microsoft.Extensions.Logging;

using Telegram.ApAzureBot.Services.Interfaces;

namespace Telegram.ApAzureBot.Services.Implementations;

public sealed class WebService : IWebService
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly string _serbianMidRfQueueId;
    private readonly string _serbianMidRfQueueCd;

    public WebService(ILogger<WebService> logger, IHttpClientFactory httpClientFactory)
    {
        _serbianMidRfQueueId = Environment.GetEnvironmentVariable("SerbianMidRfQueueId", EnvironmentVariableTarget.Process);
        ArgumentNullException.ThrowIfNull(_serbianMidRfQueueId);
        
        _serbianMidRfQueueCd = Environment.GetEnvironmentVariable("SerbianMidRfQueueCd", EnvironmentVariableTarget.Process);
        ArgumentNullException.ThrowIfNull(_serbianMidRfQueueCd);

        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public Task<string> CheckSerbianMidRf()
    {
        var client = _httpClientFactory.CreateClient("midrf");

        var url = client.BaseAddress + $"id={_serbianMidRfQueueId}&cd={_serbianMidRfQueueCd}";

        return Task.FromResult("The function is not implemented yet.");
    }
}
