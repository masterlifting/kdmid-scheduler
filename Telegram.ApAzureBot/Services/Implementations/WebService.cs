using Microsoft.Extensions.Logging;

using Telegram.ApAzureBot.Services.Interfaces;

namespace Telegram.ApAzureBot.Services.Implementations;

public sealed class WebService : IWebService
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly string _serbianMidRfSecret;

    public WebService(ILogger<WebService> logger, IHttpClientFactory httpClientFactory)
    {
        _serbianMidRfSecret = Environment.GetEnvironmentVariable("SerbianMidRfSecret", EnvironmentVariableTarget.Process);
        
        ArgumentNullException.ThrowIfNull(_serbianMidRfSecret);
        
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> CheckSerbianMidRf()
    {
        var client = _httpClientFactory.CreateClient("midrf");

        var page = await client.GetStringAsync(client.BaseAddress + _serbianMidRfSecret);

        return "The function is not implemented yet.";
    }
}
