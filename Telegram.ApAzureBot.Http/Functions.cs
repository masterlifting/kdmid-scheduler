using System.Globalization;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

using Telegram.ApAzureBot.Core.Abstractions;

namespace Telegram.ApAzureBot.Http
{
    public class Functions
    {
        private const string StartFunction = "setup";
        private const string ListenFunction = "listen";
        private const string HandleFunction = "handle";

        private readonly IClient _telegramService;

        public Functions(IClient telegramService) => _telegramService = telegramService;

        [Function(StartFunction)]
        public Task Start([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request, CancellationToken cToken)
        {
            var url = request.Url.ToString().Replace(StartFunction, HandleFunction, true, CultureInfo.InvariantCulture);
            return _telegramService.SetWebhook(url, cToken);
        }

        [Function(ListenFunction)]
        public Task Listen([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData _, CancellationToken cToken) => 
            _telegramService.ListenMessages(cToken);

        [Function(HandleFunction)]
        public async Task Handle([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request, CancellationToken cToken)
        {
            var data = await request.ReadAsStringAsync();

            ArgumentNullException.ThrowIfNull(data, "Telegram request data were not found.");

            await _telegramService.ReceiveMessage(data, cToken);
        }
    }
}
