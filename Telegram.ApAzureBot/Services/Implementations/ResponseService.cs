using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Telegram.ApAzureBot.Services.Interfaces;
using Telegram.Bot.Types;

namespace Telegram.ApAzureBot.Services.Implementations;

internal sealed class ResponseService : IResponseService
{
    ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Lazy<IProcessService>> _services;

    public ResponseService(ILogger<ResponseService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _services = new()
        {
            {"midrf", new(() => _serviceProvider.GetRequiredService<IMidRfService>())},
        };
    }

    public Task Process(Message message, CancellationToken cToken)
    {
        if (string.IsNullOrWhiteSpace(message.Text) || message.Text.Length < 2)
           throw new NotSupportedException(nameof(message));

        var text = message.Text.Trim().ToLower().AsSpan();

        return text.StartsWith("/")
            ? ProcessCommand(message.Chat.Id, text, cToken)
            : ProcessResponse(message.Chat.Id, text);
    }

    public Task SetResponse(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || message.Length < 2)
            throw new NotSupportedException(nameof(message));

        var text = message.Trim().ToLower().AsSpan();

        return text.StartsWith("/")
            ? ProcessCommand( default, text[1..], default)
            : ProcessResponse( default, text);
    }

    private Task ProcessCommand(long chatId, ReadOnlySpan<char> command, CancellationToken cToken)
    {
        var nextCommandStartIndex = command.IndexOf('/');

        var commandName = nextCommandStartIndex > 0 
            ? command[0..nextCommandStartIndex] 
            : command;
        
        return !_services.TryGetValue(commandName.ToString(), out var service)
            ? throw new NotSupportedException("This service is not supported.")
            : service.Value.Process(chatId, command[(nextCommandStartIndex + 1)..],cToken);
    }
    private Task ProcessResponse(long chatId, ReadOnlySpan<char> response)
    {
        throw new NotImplementedException("This way is not implemented yet.");
    }
}
