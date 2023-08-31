using Microsoft.Azure.Functions.Worker;

using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Worker.Models;

namespace Telegram.ApAzureBot.Worker;

public class Functions
{
    private readonly ITelegramCommandTaskService _service;
    public Functions(ITelegramCommandTaskService service) => _service = service;

    [Function("fastseek")]
    public async Task RunFastSeek([TimerTrigger("*/25 6-8 * * 1-5")] TelegramTimer timer) => await _service.Process(new[]
    {
        Core.Constants.Kdmid.Cities.Budapest,
        Core.Constants.Kdmid.Cities.Belgrade
    });
    
    [Function("slowseek")]
    public async Task RunSlowSeek([TimerTrigger("*/30 8-16 * * 1-5")] TelegramTimer timer) => await _service.Process(Array.Empty<string>());
}
