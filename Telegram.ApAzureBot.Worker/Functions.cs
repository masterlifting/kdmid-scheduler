using Microsoft.Azure.Functions.Worker;
using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Worker.Models;

namespace Telegram.ApAzureBot.Worker;

public class Functions
{
    private readonly ITelegramCommandTaskService _service;
    public Functions(ITelegramCommandTaskService service) => _service = service;

    [Function("TelegramApAzureBotWorker")]
    public Task Run([TimerTrigger("0 */1 * * * *")] TelegramTimer timer) => _service.Process();
}
