using TelegramBot.Abstractions.Models.CommandProcesses.Kdmid;

namespace TelegramBot.Abstractions.Interfaces.Services.CommandProcesses.Kdmid;

public interface IKdmidCommandProcess : ITelegramCommandProcess
{
    Task Menu(KdmidCommand command, CancellationToken cToken);
    Task Request(KdmidCommand command, CancellationToken cToken);
    Task Seek(KdmidCommand command, CancellationToken cToken);
    Task Confirm(KdmidCommand command, CancellationToken cToken);
}
