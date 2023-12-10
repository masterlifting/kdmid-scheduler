using TelegramBot.Abstractions.Interfaces.Services.CommandProcesses;
using TelegramBot.Abstractions.Interfaces.Services.CommandProcesses.Kdmid;
using TelegramBot.Abstractions.Interfaces.Services;
using TelegramBot.Abstractions.Models.Exceptions;
using TelegramBot.Abstractions.Models;

namespace TelegramBot.Services;

public sealed class TelegramCommand : Abstractions.Interfaces.Services.ITelegramCommand
{
    ILogger _logger;
    private readonly ITelegramServiceProvider _serviceProvider;
    private readonly Dictionary<string, Func<Abstractions.Interfaces.Services.CommandProcesses.ITelegramCommand>> _services;
    public TelegramCommand(ILogger<TelegramCommand> logger, ITelegramServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _services = new()
        {
            {"start", _serviceProvider.GetService<ITelegramMenuCommandProcess>},
            {"menu", _serviceProvider.GetService<ITelegramMenuCommandProcess>},
            {Constants.Kdmid.Key, _serviceProvider.GetService<IKdmidCommand>},
        };
    }

    public async Task Execute(TelegramMessage message, CancellationToken cToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message.Text) || message.Text.Length < 4)
                throw new TelegramBotException("The message is empty.");

            var text = message.Text.Trim();

            if (!text.StartsWith('/'))
                throw new TelegramBotException($"The message {text} is not a command.");

            await ProcessCommand(message.ChatId, text[1..].AsSpan(), cToken);
        }
        catch (TelegramBotException exception)
        {
            //var errorMessage = $"An error occurred while processing the command {message.Text}.";

            // This is only for MVP version
            var errorMessage = $"{message.Text}: {exception.Message}";
            _logger.Error(exception);
            await _serviceProvider.GetClient().SendMessage(new(message.ChatId, errorMessage), cToken);
        }
        catch (Exception exception)
        {
            //var errorMessage = $"An error occurred while processing the command {message.Text}.";

            // This is only for MVP version
            var errorMessage = $"{message.Text}: {exception.Message}";
            _logger.Error(new TelegramBotException(errorMessage));
            await _serviceProvider.GetClient().SendMessage(new(message.ChatId, errorMessage), cToken);
        }
    }
    private Task ProcessCommand(long chatId, ReadOnlySpan<char> command, CancellationToken cToken)
    {
        var commandParametersStartIndex = command.IndexOf('_');

        var commandName = commandParametersStartIndex > 0
            ? command[0..commandParametersStartIndex]
            : command;

        return !_services.TryGetValue(commandName.ToString(), out var service)
            ? throw new TelegramBotException($"The service {commandName} is not supported.")
            : service().Start(chatId, command[(commandParametersStartIndex + 1)..], cToken);
    }
}
