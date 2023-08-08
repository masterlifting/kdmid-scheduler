using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;
using Telegram.ApAzureBot.Infrastructure.Abstractions;
using Telegram.ApAzureBot.Core.Abstractions;

namespace Telegram.ApAzureBot.Infrastructure.Services
{
    public sealed class TelegramService : IClient
    {
        public ITelegramBotClient Client { get; }

        private readonly ILogger _logger;
        private readonly IResponseService _responseService;
        public TelegramService(ILogger<TelegramService> logger, IResponseService responseService)
        {
            _logger = logger;
            _responseService = responseService;

            var token = Environment.GetEnvironmentVariable("TelegramBotToken", EnvironmentVariableTarget.Process);

            ArgumentNullException.ThrowIfNull(token, "Telegram token was not found.");

            Client = new TelegramBotClient(token);
        }

        public Task SetWebhook(string url, CancellationToken cToken) =>
            Client.SetWebhookAsync(url, cancellationToken: cToken);
        public async Task ReceiveMessage(string data, CancellationToken cToken)
        {
            var update = JsonConvert.DeserializeObject<Update>(data);

            ArgumentNullException.ThrowIfNull(update, "Update is null");

            if (update.Type != UpdateType.Message || update.Message!.Type != MessageType.Text)
            {
                await Client.SendTextMessageAsync(update.Message!.Chat.Id, "The message type is not supported.", cancellationToken: cToken);
            }

            await _responseService.Process(update.Message, cToken);
        }
        public Task ListenMessages(CancellationToken cToken)
        {
            Client.StartReceiving(HandleUpdate, HandleError, cancellationToken: cToken);
            return Task.CompletedTask;
        }

        private Task HandleError(ITelegramBotClient client, Exception exception, CancellationToken cToken)
        {
            _logger.LogError(exception, "Error occurred while receiving a message.");
            return Task.CompletedTask;
        }
        private Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cToken) =>
            update.Type != UpdateType.Message || update.Message!.Type != MessageType.Text
                ? Client.SendTextMessageAsync(update.Message!.Chat.Id, "The message type is not supported.", cancellationToken: cToken)
                : _responseService.Process(update.Message, cToken);
    }
}
