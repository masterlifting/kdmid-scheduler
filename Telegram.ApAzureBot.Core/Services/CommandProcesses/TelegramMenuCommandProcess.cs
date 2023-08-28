using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;
using Telegram.ApAzureBot.Core.Models;

using static Telegram.ApAzureBot.Core.Constants;

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
        var menuButton = new TelegramButtons(chatId, "Choose Russian embassy.", new[]
        {
            ("Belgrade", KdmidCommandProcess.BuildCommand(Kdmid.Cities.Belgrade, Kdmid.Commands.Menu)),
            ("Budapest", KdmidCommandProcess.BuildCommand(Kdmid.Cities.Budapest, Kdmid.Commands.Menu)),
            ("Paris", KdmidCommandProcess.BuildCommand(Kdmid.Cities.Paris, Kdmid.Commands.Menu)),
            ("Bucharest", KdmidCommandProcess.BuildCommand(Kdmid.Cities.Bucharest, Kdmid.Commands.Menu))
        });

        return _telegramClient.SendButtons(menuButton, cToken);
    }
}
