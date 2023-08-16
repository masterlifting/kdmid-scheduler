using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;
using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Models;

namespace Telegram.ApAzureBot.Core.Services;

public sealed class TelegramCommand : ITelegramCommand
{
    private readonly ITelegramServiceProvider _serviceProvider;
    private readonly Dictionary<string, Lazy<ITelegramCommandProcess>> _services;

    public TelegramCommand(ITelegramServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _services = new()
        {
            {Constants.Kdmid, new(_serviceProvider.GetService<IKdmidCommandProcess>)},
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
            var telegramClient = _serviceProvider.GetTelegramClient();

            await telegramClient.SendMessage(new(message.ChatId, "Error: " + exception.Message), cToken);
        }
    }
    private Task ProcessCommand(long chatId, ReadOnlySpan<char> command, CancellationToken cToken)
    {
        var commandParametersStartIndex = command.IndexOf('/');

        var commandName = commandParametersStartIndex > 0
            ? command[0..commandParametersStartIndex]
            : command;

        return !_services.TryGetValue(commandName.ToString(), out var service)
            ? throw new NotSupportedException("Service is not supported.")
            : service.Value.Start(chatId, command[(commandParametersStartIndex + 1)..], cToken);
    }
}
