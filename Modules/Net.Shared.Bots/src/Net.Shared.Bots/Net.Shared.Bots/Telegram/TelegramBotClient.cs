using System.Globalization;

using Microsoft.Extensions.Logging;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

using Newtonsoft.Json;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using static Net.Shared.Bots.Abstractions.Constants;

namespace Net.Shared.Bots.Telegram;

public sealed class TelegramBotClient(ILogger<TelegramBotClient> logger, IBotCommand command, ITelegramBotClient client) : IBotClient
{
    private readonly ILogger _logger = logger;
    private readonly IBotCommand _command = command;
    private readonly ITelegramBotClient _client = client;

    public async Task Listen(Uri uri, CancellationToken cToken)
    {
        await _client.SetWebhookAsync(uri.ToString(), cancellationToken: cToken);
    }
    public async Task Listen(CancellationToken cToken)
    {
        await _client.DeleteWebhookAsync(true, cToken);
        
        var options = new ReceiverOptions
        {
        };

        _client.StartReceiving(HandleReceivedMessage, HandleReceivedMessageError, options, cToken);
    }
    public async Task Send(IBotMessage message, CancellationToken cToken)
    {
        await HandleSendingMessage(message, cToken);
    }
    public async Task Receive(string data, CancellationToken cToken)
    {
        var update = JsonConvert.DeserializeObject<Update>(data);

        ArgumentNullException.ThrowIfNull(update, "Received data was not recognized.");

        await HandleReceivedMessage(_client, update, cToken);
    }

    Task HandleReceivedMessage(ITelegramBotClient client, Update update, CancellationToken cToken)
    {
        ArgumentNullException.ThrowIfNull(update, "Received data was not recognized.");

        var(type, data, chatId) = update.Type switch
        {
            UpdateType.Message => HandleReceivedMessageType(update.Message),
            UpdateType.EditedMessage => HandleReceivedMessageType(update.Message),
            UpdateType.ChannelPost => HandleReceivedMessageType(update.Message),
            UpdateType.EditedChannelPost => HandleReceivedMessageType(update.Message),
            UpdateType.CallbackQuery => (BotMessageType.Command, update.CallbackQuery?.Data, update.CallbackQuery?.Message?.Chat.Id),
            UpdateType.InlineQuery => (BotMessageType.Command, update.InlineQuery?.Query, update.InlineQuery?.From.Id),
            UpdateType.ChosenInlineResult => (BotMessageType.Command, update.ChosenInlineResult?.Query, update.ChosenInlineResult?.From.Id),
            _ => throw new NotSupportedException($"Update type {update.Type} is not supported.")
        };

        if(chatId is null)
            throw new InvalidOperationException("Received chat id is empty.");

        if(string.IsNullOrWhiteSpace(data))
            throw new InvalidOperationException("Received data is empty.");

        var message = new BotMessage
        {
            ChatId = chatId.Value,
            Data = data,
            Type = type
        };

        return _command.Process(message, cToken);

        static (BotMessageType type, string? data, long? chatId) HandleReceivedMessageType(Message? message)
        {
            ArgumentNullException.ThrowIfNull(message, "Received data was not recognized.");

            return message.Type switch
            {
                MessageType.Text => (BotMessageType.Command, message.Text, message.Chat.Id),
                MessageType.Photo => (BotMessageType.Image, message.Photo?.FirstOrDefault()?.FileId, message.Chat.Id),
                MessageType.Audio => (BotMessageType.Audio, message.Audio?.FileId, message.Chat.Id),
                MessageType.Video => (BotMessageType.Video, message.Video?.FileId, message.Chat.Id),
                MessageType.Voice => (BotMessageType.Voice, message.Voice?.FileId, message.Chat.Id),
                MessageType.Document => (BotMessageType.Document, message.Document?.FileId, message.Chat.Id),
                MessageType.Location => (BotMessageType.Location, $"{message.Location?.Longitude.ToString(CultureInfo.InvariantCulture)}, {message.Location?.Latitude.ToString(CultureInfo.InvariantCulture)}", message.Chat.Id),
                MessageType.Contact => (BotMessageType.Contact, message.Contact?.PhoneNumber, message.Chat.Id),
                _ => throw new NotSupportedException($"Message type {message.Type} is not supported.")
            };
        }
    }
    Task HandleReceivedMessageError(ITelegramBotClient client, Exception exception, CancellationToken cToken)
    {
        _logger.LogError(exception, "Received message error.");
        return Task.CompletedTask;
    }
    Task HandleSendingMessage(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
