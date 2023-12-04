using Microsoft.Extensions.Logging;

using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;
using Telegram.ApAzureBot.Core.Exceptions;
using Telegram.ApAzureBot.Core.Models;

using Net.Shared.Extensions;

namespace Telegram.ApAzureBot.Core.Services;

public sealed class TelegramCommand : ITelegramCommand
{
    ILogger _logger;
    private readonly ITelegramServiceProvider _serviceProvider;
    private readonly Dictionary<string, Func<ITelegramCommandProcess>> _services;
    public TelegramCommand(ILogger<TelegramCommand> logger, ITelegramServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _services = new()
        {
            {"start", _serviceProvider.GetService<ITelegramMenuCommandProcess>},
            {"menu", _serviceProvider.GetService<ITelegramMenuCommandProcess>},
            {Constants.Kdmid.Key, _serviceProvider.GetService<IKdmidCommandProcess>},
        };
    }

    public async Task Execute(TelegramMessage message, CancellationToken cToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message.Text) || message.Text.Length < 4)
                throw new ApAzureBotCoreException("The message is empty.");

            var text = message.Text.Trim();

            if (!text.StartsWith('/'))
                throw new ApAzureBotCoreException($"The message {text} is not a command.");

            await ProcessCommand(message.ChatId, text[1..].AsSpan(), cToken);
        }
        catch (ApAzureBotCoreException exception)
        {
            //var errorMessage = $"An error occurred while processing the command {message.Text}.";

            // This is only for MVP version
            var errorMessage = $"{message.Text}: {exception.Message}";
            _logger.Error(exception);
            await _serviceProvider.GetTelegramClient().SendMessage(new(message.ChatId, errorMessage), cToken);
        }
        catch (Exception exception)
        {
            //var errorMessage = $"An error occurred while processing the command {message.Text}.";

            // This is only for MVP version
            var errorMessage = $"{message.Text}: {exception.Message}";
            _logger.Error(new ApAzureBotCoreException(errorMessage));
            await _serviceProvider.GetTelegramClient().SendMessage(new(message.ChatId, errorMessage), cToken);
        }
    }
    private Task ProcessCommand(long chatId, ReadOnlySpan<char> command, CancellationToken cToken)
    {
        var commandParametersStartIndex = command.IndexOf('_');

        var commandName = commandParametersStartIndex > 0
            ? command[0..commandParametersStartIndex]
            : command;

        return !_services.TryGetValue(commandName.ToString(), out var service)
            ? throw new ApAzureBotCoreException($"The service {commandName} is not supported.")
            : service().Start(chatId, command[(commandParametersStartIndex + 1)..], cToken);
    }
}
