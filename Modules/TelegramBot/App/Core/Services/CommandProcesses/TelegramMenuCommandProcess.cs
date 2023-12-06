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
        var menuButton = new TelegramButtons(
            chatId
            , "Choose Russian embassy."
            , KdmidCommandProcess.Cities
                .OrderBy(x => x.Value.Name)
                .Select(x => (x.Value.Name, KdmidCommandProcess.BuildCommand(x.Value.Id, Kdmid.Commands.Menu)))
            , ButtonStyle.VerticallyFlex
        );

        return _telegramClient.SendButtons(menuButton, cToken);
    }
}
