using Microsoft.Extensions.Logging;

using Telegram.ApAzureBot.Services.Interfaces;
using Telegram.Bot.Types;

namespace Telegram.ApAzureBot.Services.Implementations
{
    internal sealed class ResponseService : IResponseService
    {
        ILogger _logger;
        private readonly IWebService _webService;

        public ResponseService(ILogger<ResponseService> logger, IWebService webService)
        {
            _logger = logger;
            _webService = webService;
        }

        public Task<string> CheckMidRf(Update request)
        {
            var message = request.Message?.Text ?? throw new ArgumentNullException(nameof(request));
            
            try
            {
                return message switch
                {
                    "/midrf" => _webService.CheckSerbianMidRf(),
                    _ => Task.FromResult("I don't know what you want from me 😢")
                };
            }
            catch
            {
                return Task.FromResult($"While handling '{message}', something went wrong.");
            }
        }
    }
}
