using Microsoft.Extensions.Logging;

using System.Data;

using Telegram.ApAzureBot.Services.Interfaces;
using Telegram.Bot.Types;

namespace Telegram.ApAzureBot.Services.Implementations
{
    internal sealed class ResponseService : IResponseService
    {
        ILogger _logger;
        public ResponseService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ResponseService>();
        }

        public string Create(Update request)
        {
            try
            {
                var message = request.Message?.Text ?? throw new ArgumentNullException(nameof(request));

                var result = new DataTable().Compute(message, null)?.ToString();

                return result is null
                    ? throw new ArgumentNullException(result, nameof(result))
                    : result;
            }
            catch
            {
                return $"Dear human, I can solve math for you, try '2 + 2 * 3' 👀";
            }
        }
    }
}
