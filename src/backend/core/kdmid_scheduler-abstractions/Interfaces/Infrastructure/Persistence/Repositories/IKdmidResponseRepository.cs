using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Repositories;

public interface IKdmidResponseRepository
{
    Task Create(string commandName, string chatId, City city, KdmidId kdmidId, CancellationToken cToken);
    Task Update(Command command, string chatId, City city, KdmidId kdmidId, CancellationToken cToken);
    Task Clear(string chatId, Command command, City city, KdmidId kdmidId, CancellationToken cToken);
    Task<string> GetInfo(string chatId, Command command, CancellationToken cToken);
}
