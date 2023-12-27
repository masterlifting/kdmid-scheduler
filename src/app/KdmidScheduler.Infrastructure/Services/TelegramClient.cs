﻿//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;

//using Net.Shared.Extensions;

//using Newtonsoft.Json;

//using Telegram.Bot;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Types.ReplyMarkups;

//namespace TelegramBot.Infrastructure.Services;

//public sealed class TelegramClient : ITelegramClient
//{
//    private readonly ILogger _logger;
//    private readonly ITelegramBotClient _client;
//    private readonly ITelegramCommand _command;
//    public TelegramClient(ILogger<TelegramClient> logger, ITelegramCommand telegramCommand, IConfiguration configuration)
//    {
//        _logger = logger;
//        _command = telegramCommand;

//        var token = configuration["TelegramBotToken"];

//        ArgumentNullException.ThrowIfNull(token, "Telegram token was not found.");

//        _client = new TelegramBotClient(token);
//    }

//    public Task SetWebhook(string url, CancellationToken cToken) =>
//        _client.SetWebhookAsync(url, cancellationToken: cToken);
//    public Task ReceiveMessage(string data, CancellationToken cToken)
//    {
//        var update = JsonConvert.DeserializeObject<Update>(data);

//        ArgumentNullException.ThrowIfNull(update, "Received data was not recognized.");

//        return HandleMessage(_client, update, cToken);
//    }
//    public async Task ListenMessages(CancellationToken cToken)
//    {
//        await _client.DeleteWebhookAsync(true, cToken);
//        _client.StartReceiving(HandleMessage, HandleError, cancellationToken: cToken);
//    }

//    public Task SendMessage(TelegramMessage message, CancellationToken cToken) =>
//        _client.SendTextMessageAsync(message.ChatId, message.Text, cancellationToken: cToken);
//    public async Task SendPhoto(TelegramPhoto photo, CancellationToken cToken)
//    {
//        using var stream = new MemoryStream(photo.Payload);

//        var data = InputFile.FromStream(stream, photo.Name);

//        await _client.SendPhotoAsync(photo.ChatId, data, caption: photo.Description, cancellationToken: cToken);
//    }
//    public async Task SendButtons(TelegramButtons button, CancellationToken cToken)
//    {
//        InlineKeyboardMarkup keyboard = button.Style switch
//        {
//            ButtonStyle.Horizontally => new(button.Items.Select(x => InlineKeyboardButton.WithCallbackData(x.Name, x.Callback))),
//            ButtonStyle.VerticallyStrict => new(button.Items.Select(x => new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(x.Name, x.Callback) })),
//            ButtonStyle.VerticallyFlex => button.Items.Count() % 2 == 0
//                ? new(GetButtonPairs(button.Items.Select(x => InlineKeyboardButton.WithCallbackData(x.Name, x.Callback)).ToArray()))
//                : new(button.Items.Select(x => new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(x.Name, x.Callback) })),
//            _ => throw new ApAzureBotInfrastructureException($"Button style {button.Style} is not supported.")
//        };

//        await _client.SendTextMessageAsync(button.ChatId, button.Text, replyMarkup: keyboard, cancellationToken: cToken);

//        static IEnumerable<InlineKeyboardButton[]> GetButtonPairs(InlineKeyboardButton[] buttons)
//        {
//            var pairs = new List<InlineKeyboardButton[]>(buttons.Length / 2);

//            for (var i = 0; i < buttons.Length; i += 2)
//            {
//                pairs.Add(new InlineKeyboardButton[] { buttons[i], buttons[i + 1] });
//            }

//            return pairs;
//        }
//    }

//    #region Private methods
//    private Task HandleError(ITelegramBotClient client, Exception exception, CancellationToken cToken)
//    {
//        _logger.Error(new ApAzureBotInfrastructureException(exception));
//        return Task.CompletedTask;
//    }
//    private Task HandleMessage(ITelegramBotClient client, Update update, CancellationToken cToken) => update.Type switch
//    {
//        UpdateType.Message => update.Message!.Type != MessageType.Text
//            ? _client.SendTextMessageAsync(update.Message!.Chat.Id, "Message type is not supported.", cancellationToken: cToken)
//            : _command.Execute(new(update.Message.Chat.Id, update.Message.Text!), cToken),
//        UpdateType.CallbackQuery => update.CallbackQuery!.Message!.Type != MessageType.Text
//            ? _client.SendTextMessageAsync(update.CallbackQuery!.Message!.Chat.Id, "Message type is not supported.", cancellationToken: cToken)
//            : _command.Execute(new(update.CallbackQuery!.Message!.Chat.Id, update.CallbackQuery!.Data!), cToken),
//        _ => _client.SendTextMessageAsync(update.Message!.Chat.Id, "Message type is not supported.", cancellationToken: cToken),
//    };
//    #endregion
//}
