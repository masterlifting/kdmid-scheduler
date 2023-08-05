using Microsoft.Extensions.DependencyInjection;

using Telegram.ApAzureBot.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Telegram.ApAzureBot.Services.Implementations;

internal sealed class ResponseService : IResponseService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Lazy<IProcessService>> _services;

    public ResponseService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _services = new()
        {
            {Constants.Kdmid, new(() => _serviceProvider.GetRequiredService<IKdmidService>())},
        };
    }

    public Task Process(Message message, CancellationToken cToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message.Text) || message.Text.Length < 2)
                throw new NotSupportedException("This message is not supported.");

            var text = message.Text.Trim().ToLower().AsSpan();

            return text.StartsWith("/")
                ? Process(message.Chat.Id, text[1..], cToken)
                : throw new NotSupportedException("This message is not command.");
        }
        catch (Exception exception)
        {
            var telegramService = _serviceProvider.GetRequiredService<ITelegramService>();
            return telegramService.Client.SendTextMessageAsync(message.Chat.Id, exception.Message, cancellationToken: cToken);
        }
    }
    private Task Process(long chatId, ReadOnlySpan<char> command, CancellationToken cToken)
    {
        var nextCommandStartIndex = command.IndexOf('/');

        var commandName = nextCommandStartIndex > 0
            ? command[0..nextCommandStartIndex]
            : command;

        return !_services.TryGetValue(commandName.ToString(), out var service)
            ? throw new NotSupportedException("The service is not supported.")
            : service.Value.Process(chatId, command[(nextCommandStartIndex + 1)..], cToken);
    }
}
