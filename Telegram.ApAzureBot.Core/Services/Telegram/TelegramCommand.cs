using Telegram.ApAzureBot.Core.Abstractions.Services.CommandServices;
using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Persistence.NoSql;

namespace Telegram.ApAzureBot.Core.Services.Telegram;

public sealed class TelegramCommand : ITelegramCommand
{
    private readonly ITelegramServiceProvider _serviceProvider;
    private readonly Dictionary<string, Lazy<ITelegramCommandProcess>> _services;

    public TelegramCommand(ITelegramServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _services = new()
        {
            {Constants.Kdmid, new(() => _serviceProvider.GetService<IKdmidService>())},
        };
    }

    public async Task Process(TelegramMessage message, CancellationToken cToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message.Text) || message.Text.Length < 4)
                throw new NotSupportedException("Message is not supported.");

            var text = message.Text.Trim();

            if (!text.StartsWith('/'))
                throw new NotSupportedException("Message is not a command.");

            await Process(message.ChatId, text[1..].AsSpan(), cToken);
        }
        catch (Exception exception)
        {
            var telegramClient = _serviceProvider.GetTelegramClient();

            await telegramClient.SendMessage(new(message.ChatId, "Error: " + exception.Message), cToken);
        }
    }
    private Task Process(long chatId, ReadOnlySpan<char> command, CancellationToken cToken)
    {
        var nextCommandStartIndex = command.IndexOf('/');

        var commandName = nextCommandStartIndex > 0
            ? command[0..nextCommandStartIndex]
            : command;

        return !_services.TryGetValue(commandName.ToString(), out var service)
            ? throw new NotSupportedException("Service is not supported.")
            : service.Value.Start(chatId, command[(nextCommandStartIndex + 1)..], cToken);
    }
}
