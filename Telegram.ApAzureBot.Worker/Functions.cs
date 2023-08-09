using Microsoft.Azure.Functions.Worker;
using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Persistence.NoSql;
using Telegram.ApAzureBot.Worker.Models;

namespace Telegram.ApAzureBot.Worker;

public class Functions
{
    private readonly ITelegramCommand _command;

    public Functions(ITelegramCommand telegramCommand)
    {
        _command = telegramCommand;
    }

    [Function("TelegramApAzureBotWorker")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TelegramTimer timer, CancellationToken token)
    {
        var message = new TelegramMessage(0, "");
        
        await _command.Process(message, token);
    }
}
