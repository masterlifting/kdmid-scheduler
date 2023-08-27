using Telegram.ApAzureBot.Core.Models.CommandProcesses.Kdmid;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

public interface IKdmidCommandProcess : ITelegramCommandProcess
{
    Task Request(KdmidCommand command, CancellationToken cToken);
    Task Confirm(KdmidCommand command, CancellationToken cToken);
}
