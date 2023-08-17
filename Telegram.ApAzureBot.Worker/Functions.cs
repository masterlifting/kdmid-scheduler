using Microsoft.Azure.Functions.Worker;
using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Worker.Models;

using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Telegram.ApAzureBot.Worker;

public class Functions
{
    private readonly ITelegramCommandTaskService _service;
    public Functions(ITelegramCommandTaskService service) => _service = service;

    [Function("TelegramApAzureBotWorker")]
    public Task Run([TimerTrigger("*/30 8-17 * * 1-5")] TelegramTimer timer) => _service.Process();
}
