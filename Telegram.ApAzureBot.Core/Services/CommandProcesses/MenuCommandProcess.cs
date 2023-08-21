using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;
using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Models;

namespace Telegram.ApAzureBot.Core.Services.CommandProcesses
{
    public sealed class MenuCommandProcess : IMenuCommandProcess
    {
        private readonly ITelegramClient _telegramClient;

        public MenuCommandProcess(ITelegramClient telegramClient)
        {
            _telegramClient = telegramClient;
        }
        public Task Start(long chatId, ReadOnlySpan<char> message, CancellationToken cToken)
        {
            var menuButton = new TelegramButtons(chatId, "Choose Russian embassy:", new[]
            {
                    ("Belgrade", "/kdmid_blgd_sch"),
                    ("Budapest", "/kdmid_bdpt_sch")
            });
            
            return _telegramClient.SendButtons(menuButton, cToken);
        }
    }
}
