using TelegramBot.Abstractions.Models.CommandProcesses.Kdmid;

namespace TelegramBot.Abstractions.Interfaces.Services.CommandProcesses.Kdmid;

public interface IKdmidCommand : ITelegramCommand
{
    Task Menu(Command command, CancellationToken cToken);
    Task Request(Command command, CancellationToken cToken);
    Task Seek(Command command, CancellationToken cToken);
    Task Confirm(Command command, CancellationToken cToken);
}
