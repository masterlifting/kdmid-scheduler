﻿using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;
using Telegram.ApAzureBot.Core.Models;

namespace Telegram.ApAzureBot.Core.Services.CommandProcesses;

public sealed class TelegramMenuCommandProcess : ITelegramMenuCommandProcess
{
    private readonly ITelegramClient _telegramClient;

    public TelegramMenuCommandProcess(ITelegramClient telegramClient)
    {
        _telegramClient = telegramClient;
    }
    public Task Start(long chatId, ReadOnlySpan<char> message, CancellationToken cToken)
    {
        var menuButton = new TelegramButtons(chatId, "Choose Russian embassy:", new[]
        {
            ("Belgrade", KdmidCommandProcess.GetMenuCommand(KdmidCommandProcess.Belgrade)),
            ("Budapest", KdmidCommandProcess.GetMenuCommand(KdmidCommandProcess.Budapest)),
            ("Paris", KdmidCommandProcess.GetMenuCommand(KdmidCommandProcess.Paris)),
            ("Bucharest", KdmidCommandProcess.GetMenuCommand(KdmidCommandProcess.Bucharest) )
        });

        return _telegramClient.SendButtons(menuButton, cToken);
    }
}
