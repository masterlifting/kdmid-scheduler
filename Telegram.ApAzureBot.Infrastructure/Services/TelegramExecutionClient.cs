﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Net.Shared.Extensions;

using Newtonsoft.Json;

using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Models;
using Telegram.ApAzureBot.Infrastructure.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.ApAzureBot.Infrastructure.Services;

public sealed class TelegramExecutionClient : ITelegramClient
{
    private readonly ILogger _logger;
    private readonly ITelegramBotClient _client;
    private readonly ITelegramCommand _command;
    public TelegramExecutionClient(ILogger<TelegramExecutionClient> logger, ITelegramCommand telegramCommand, IConfiguration configuration)
    {
        _logger = logger;
        _command = telegramCommand;

        var token = configuration["TelegramBotToken"];

        ArgumentNullException.ThrowIfNull(token, "Telegram token was not found.");

        _client = new TelegramBotClient(token);
    }

    public Task SetWebhook(string url, CancellationToken cToken) =>
        _client.SetWebhookAsync(url, cancellationToken: cToken);
    public async Task ReceiveMessage(string data, CancellationToken cToken)
    {
        var update = JsonConvert.DeserializeObject<Update>(data);

        ArgumentNullException.ThrowIfNull(update, "Received data is not recognized.");

        if (update.Type != UpdateType.Message || update.Message!.Type != MessageType.Text)
        {
            await _client.SendTextMessageAsync(update.Message!.Chat.Id, "Message type is not supported.", cancellationToken: cToken);
        }

        await _command.Execute(new(update.Message.Chat.Id, update.Message.Text!), cToken);
    }
    public Task ListenMessages(CancellationToken cToken)
    {
        _client.StartReceiving(HandleListenerReceiving, HandleListenerError, cancellationToken: cToken);
        return Task.CompletedTask;
    }

    public Task SendMessage(TelegramMessage message, CancellationToken cToken) =>
        _client.SendTextMessageAsync(message.ChatId, message.Text, cancellationToken: cToken);
    public async Task SendPhoto(TelegramPhoto photo, CancellationToken cToken)
    {
        using var stream = new MemoryStream(photo.Payload);

        var data = InputFile.FromStream(stream, photo.Name);

        await _client.SendPhotoAsync(photo.ChatId, data, caption: photo.Description, cancellationToken: cToken);
    }

    #region Private methods
    private Task HandleListenerError(ITelegramBotClient client, Exception exception, CancellationToken cToken)
    {
        _logger.Error(new ApAzureBotInfrastructureException(exception));
        return Task.CompletedTask;
    }
    private Task HandleListenerReceiving(ITelegramBotClient client, Update update, CancellationToken cToken) =>
        update.Type != UpdateType.Message || update.Message!.Type != MessageType.Text
            ? _client.SendTextMessageAsync(update.Message!.Chat.Id, "Message type is not supported.", cancellationToken: cToken)
            : _command.Execute(new(update.Message.Chat.Id, update.Message.Text!), cToken);
    #endregion
}
