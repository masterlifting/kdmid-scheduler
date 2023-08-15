﻿using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;
using Telegram.ApAzureBot.Core.Persistence.NoSql;
using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Microsoft.Extensions.Configuration;
using Telegram.ApAzureBot.Core.Persistence;

namespace Telegram.ApAzureBot.Infrastructure.Services.Telegram;

public sealed class TelegramSchedulingClient : ITelegramClient
{
    private readonly ILogger _logger;
    private readonly ITelegramBotClient _client;
    private readonly ITelegramCommandTaskRepository _repository;
    public TelegramSchedulingClient(ILogger<TelegramSchedulingClient> logger, ITelegramCommandTaskRepository repository, IConfiguration configuration)
    {
        _logger = logger;
        _repository = repository;

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

        var message = new TelegramMessage(update.Message.Chat.Id, update.Message.Text!);
        
        var task = new TelegramCommandTask()
        {
            Message = message,
        };

        await _repository.CreateCommandTask(task, cToken);
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
        _logger.LogError(exception, "Error occurred while receiving a message.");
        return Task.CompletedTask;
    }
    private Task HandleListenerReceiving(ITelegramBotClient client, Update update, CancellationToken cToken)
    {
        if (update.Type != UpdateType.Message || update.Message!.Type != MessageType.Text)
        {
            return _client.SendTextMessageAsync(update.Message!.Chat.Id, "Message type is not supported.", cancellationToken: cToken);
        }
        else
        {
            var message = new TelegramMessage(update.Message.Chat.Id, update.Message.Text!);

            var task = new TelegramCommandTask()
            {
                Message = message,
            };

            return _repository.CreateCommandTask(task, cToken);
        }
    }
    #endregion
}
