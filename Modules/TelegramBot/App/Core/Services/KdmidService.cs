using Microsoft.Extensions.Logging;

using Net.Shared.Bots.Abstractions.Interfaces;

using TelegramBot.Abstractions.Interfaces.Services.Kdmid;

namespace KdmidScheduler.Services;

public sealed class KdmidService(ILogger<KdmidService> logger, IBotClient botClient, IBotCommandProvider commandProvider) : IKdmidService
{
    private readonly IBotClient _botClient = botClient;
    private readonly ILogger<KdmidService> _logger = logger;
    private readonly IBotCommandProvider _commandProvider = commandProvider;

    public Task HandleCommand(string chatId, IBotCommand command)
    {
        throw new NotImplementedException();
    }
}
