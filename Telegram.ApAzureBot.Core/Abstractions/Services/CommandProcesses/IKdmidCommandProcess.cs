using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Models.CommandProcesses;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;

public interface IKdmidCommandProcess : ITelegramCommandProcess
{
    Task Schedule(KdmidCommand command, CancellationToken cToken);
    Task Check(KdmidCommand command, CancellationToken cToken);
    Task Confirm(KdmidCommand command, CancellationToken cToken);
}
