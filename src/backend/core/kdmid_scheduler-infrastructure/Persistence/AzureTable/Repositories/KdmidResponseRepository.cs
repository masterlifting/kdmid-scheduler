using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Repositories;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.AzureTable.v1;
using KdmidScheduler.Infrastructure.Persistence.AzureTable.Contexts;

using Microsoft.Extensions.Options;
using Net.Shared.Abstractions.Models.Settings;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Persistence.Repositories.AzureTable;

namespace KdmidScheduler.Infrastructure.Persistence.AzureTable.Repositories;

public sealed class KdmidResponseRepository(
    IOptions<CorrelationSettings> correlationOptions,
    IBotCommandsStore commandsStore,
    AzureTableWriterRepository<KdmidPersistenceContext, KdmidAvailableDates> writer
    ) : IKdmidResponseRepository
{
    private readonly Guid _correlationId = correlationOptions.Value.Id;
    private readonly IBotCommandsStore _commandsStore = commandsStore;
    private readonly AzureTableWriterRepository<KdmidPersistenceContext, KdmidAvailableDates> _writer = writer;

    public Task Create(string commandName, string chatId, City city, KdmidId kdmidId, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task Update(Command command, string chatId, City city, KdmidId kdmidId, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task Clear(string chatId, Guid commandId, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
