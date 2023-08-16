using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;

public interface IKdmidCommandProcess : ITelegramCommandProcess
{
    Task Schedule(long chatId, string city, string parameters, CancellationToken cToken);
    Task Captcha(long chatId, string city, string parameters, CancellationToken cToken);
    Task Confirm(long chatId, string city, string parameters, CancellationToken cToken);
}
