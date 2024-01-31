using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Repositories;

public interface IKdmidResponseRepository
{
    Task CreateCommand(string commandName, string chatId, City city, KdmidId kdmidId, CancellationToken cToken);
    Task UpdateCommand(Command command, string chatId, City city, KdmidId kdmidId, CancellationToken cToken);
    Task DeleteCommand(string chatId, Guid commandId, CancellationToken cToken);
}
