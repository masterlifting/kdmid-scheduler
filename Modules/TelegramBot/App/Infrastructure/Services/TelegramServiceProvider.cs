﻿using Microsoft.Extensions.DependencyInjection;
using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;

namespace Telegram.ApAzureBot.Infrastructure.Services;

public sealed class TelegramServiceProvider : ITelegramServiceProvider
{
    private readonly IServiceProvider _serviceProvider;
    public TelegramServiceProvider(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public T GetService<T>() where T : ITelegramCommandProcess => _serviceProvider.GetRequiredService<T>();
    public ITelegramClient GetTelegramClient() => _serviceProvider.GetRequiredService<ITelegramClient>();
}
