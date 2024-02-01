using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Repositories;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.AzureTable.v1;
using KdmidScheduler.Infrastructure.Persistence.AzureTable.Contexts;

using Net.Shared.Persistence.Repositories.AzureTable;

namespace KdmidScheduler.Infrastructure.Persistence.AzureTable.Repositories.Web;

public sealed class KdmidRequestHttpClientCache(
    AzureTableWriterRepository<KdmidPersistenceContext, KdmidRequestCache> writer,
    AzureTableReaderRepository<KdmidPersistenceContext, KdmidRequestCache> reader) : IKdmidRequestHttpClientCache
{
    private readonly AzureTableWriterRepository<KdmidPersistenceContext, KdmidRequestCache> _writer = writer;
    private readonly AzureTableReaderRepository<KdmidPersistenceContext, KdmidRequestCache> _reader = reader;

    public Task SetSessionId(City city, KdmidId kdmidId, string sessionId, ushort keepAlive, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task<string> GetSessionId(City city, KdmidId kdmidId, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    public Task SetHeaders(City city, KdmidId kdmidId, Dictionary<string, string> headers, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task<Dictionary<string, string>> GetHeaders(City city, KdmidId kdmidId, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    public Task Clear(City city, KdmidId kdmidId, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
