﻿using Microsoft.Azure.Functions.Worker;

using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Worker.Models;

namespace Telegram.ApAzureBot.Worker;

public class Functions
{
    private readonly ITelegramCommandTaskService _service;
    public Functions(ITelegramCommandTaskService service) => _service = service;

    [Function("fastseek")]
    public async Task RunFastSeek([TimerTrigger("*/10 6-7 * * 1-5")] TelegramTimer timer) => await _service.Process();
    
    [Function("slowseek")]
    public async Task RunSlowSeek([TimerTrigger("0 8-15 * * 1-5")] TelegramTimer timer) => await _service.Process();
}
