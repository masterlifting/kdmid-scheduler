﻿using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;
using Telegram.ApAzureBot.Core.Exceptions;
using Telegram.ApAzureBot.Core.Models;

namespace Telegram.ApAzureBot.Core.Services;

public sealed class TelegramCommand : ITelegramCommand
{
    private readonly ITelegramServiceProvider _serviceProvider;
    private readonly Dictionary<string, Func<ITelegramCommandProcess>> _services;
    public TelegramCommand(ITelegramServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _services = new()
        {
            {"menu", _serviceProvider.GetService<ITelegramMenuCommandProcess>},
            {Constants.Kdmid, _serviceProvider.GetService<IKdmidCommandProcess>},
        };
    }

    public async Task Execute(TelegramMessage message, CancellationToken cToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message.Text) || message.Text.Length < 4)
                throw new NotSupportedException("Message is not supported.");

            var text = message.Text.Trim();

            if (!text.StartsWith('/'))
                throw new NotSupportedException("Message is not a command.");

            await ProcessCommand(message.ChatId, text[1..].AsSpan(), cToken);
        }
        catch (Exception exception)
        {
            throw new ApAzureBotCoreException(exception);
        }
    }
    private Task ProcessCommand(long chatId, ReadOnlySpan<char> command, CancellationToken cToken)
    {
        var commandParametersStartIndex = command.IndexOf('_');

        var commandName = commandParametersStartIndex > 0
            ? command[0..commandParametersStartIndex]
            : command;

        return !_services.TryGetValue(commandName.ToString(), out var service)
            ? throw new NotSupportedException("Service is not supported.")
            : service().Start(chatId, command[(commandParametersStartIndex + 1)..], cToken);
    }
}
