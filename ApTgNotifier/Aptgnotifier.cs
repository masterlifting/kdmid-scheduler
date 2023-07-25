using System.Data;
using System.Globalization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ApTgNotifier
{
    public class Aptgnotifier
    {
        private readonly ILogger _logger;
        private readonly TelegramBotClient _botClient;

        public Aptgnotifier(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Aptgnotifier>();

            var token = Environment.GetEnvironmentVariable("TelegramBotToken", EnvironmentVariableTarget.Process);

            ArgumentNullException.ThrowIfNull(token, "TelegramBotToken is not set");

            _botClient = new TelegramBotClient(token);
        }

        private const string SetUpFunctionName = "setup";
        private const string UpdateFunctionName = "handleupdate";

        [Function(SetUpFunctionName)]
        public Task Setup([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData request)
        {
            var requestUrl = request.Url.ToString();

            var webHookUrl = requestUrl.Replace(SetUpFunctionName, UpdateFunctionName, true, CultureInfo.InvariantCulture);

            return _botClient.SetWebhookAsync(webHookUrl);
        }

        [Function(UpdateFunctionName)]
        public async Task HandleUpdate([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            var request = await req.ReadAsStringAsync();

            ArgumentNullException.ThrowIfNull(request, "Request is null");

            var update = JsonConvert.DeserializeObject<Update>(request);

            ArgumentNullException.ThrowIfNull(update, "Update is null");

            if (update.Type != UpdateType.Message || update.Message!.Type != MessageType.Text)
                return;

            var message = GetBotResponseForInput(update.Message.Text);

            await _botClient.SendTextMessageAsync(update.Message.Chat.Id, message);
        }

        private static string GetBotResponseForInput(string? text)
        {
            try
            {
                if (text is null)
                    throw new ArgumentNullException(nameof(text));

                if (text.Contains("pod bay doors", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "I'm sorry Dave, I'm afraid I can't do that 🛰";
                }

                var result = new DataTable().Compute(text, null)?.ToString();

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
