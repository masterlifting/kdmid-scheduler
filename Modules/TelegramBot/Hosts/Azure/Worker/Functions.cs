using System.Globalization;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;

using Telegram.ApAzureBot.Core.Abstractions.Services;

using TelegramBot.Azure.Models;

namespace TelegramBot.Azure;

public class Functions(ITelegramCommandTaskService service)
{
    private readonly ITelegramCommandTaskService _service = service;

    private const string FastSeekFunction = "fast-seek";
    private const string SlowSeekFunction = "slow-seek";

    [Function(FastSeekFunction)]
    public async Task RunFastSeek([TimerTrigger("15,40 6-7 * * 1-5")] TelegramTimer timer) => await _service.Process(
    [
        TelegramBot.Models.Constants.Kdmid.Cities.Budapest,
        TelegramBot.Models.Constants.Kdmid.Cities.Belgrade
    ]);

    [Function(SlowSeekFunction)]
    public async Task RunSlowSeek([TimerTrigger("0/30 7-14 * * 1-5")] TelegramTimer timer) => await _service.Process([]);
}
