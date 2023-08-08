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

    public async Task Process(Message message, CancellationToken cToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message.Text) || message.Text.Length < 4)
                throw new NotSupportedException("Message is not supported.");

            var text = message.Text.Trim().ToLower();

            if(!text.StartsWith("/"))
                throw new NotSupportedException("Message is not a command.");

            await Process(message.Chat.Id, text[1..].AsSpan(), cToken);
        }
        catch (Exception exception)
        {
            var telegramService = _serviceProvider.GetRequiredService<ITelegramService>();
            await telegramService.Client.SendTextMessageAsync(message.Chat.Id, "Error: " + exception.Message, cancellationToken: cToken);
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
            : service.Value.Process(chatId, command[(nextCommandStartIndex + 1)..], cToken);
    }
}
