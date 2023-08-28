using Microsoft.Azure.Functions.Worker;

using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Worker.Models;

namespace Telegram.ApAzureBot.Worker;

public class Functions
{
    private readonly ITelegramCommandTaskService _service;
    public Functions(ITelegramCommandTaskService service) => _service = service;

    [Function("handle")]
    public async Task Run([TimerTrigger("20-59/30 6-14 * * 1-5")] TelegramTimer timer) => await _service.Process();
}
