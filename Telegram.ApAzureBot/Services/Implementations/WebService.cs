using Microsoft.Extensions.Logging;

using Telegram.ApAzureBot.Services.Interfaces;

namespace Telegram.ApAzureBot.Services.Implementations
{
    public sealed class WebService : IWebService
    {
        private readonly ILogger<WebService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public WebService(ILogger<WebService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public Task<string> CheckSerbianMidRf()
        {
            return Task.FromResult("The function is not implemented yet.");
        }
    }
}
