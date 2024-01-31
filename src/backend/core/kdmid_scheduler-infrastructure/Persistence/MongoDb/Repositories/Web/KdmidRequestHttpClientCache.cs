using System.Linq.Expressions;

using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Repositories;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;
using KdmidScheduler.Infrastructure.Persistence.MongoDb.Contexts;

using Net.Shared.Persistence.Repositories.MongoDb;

namespace KdmidScheduler.Infrastructure.Persistence.MongoDb.Repositories.Web;

public sealed class KdmidRequestHttpClientCache(
    MongoDbWriterRepository<KdmidPersistenceContext, KdmidRequestCache> writer,
    MongoDbReaderRepository<KdmidPersistenceContext, KdmidRequestCache> reader) : IKdmidRequestHttpClientCache
{
    private readonly MongoDbWriterRepository<KdmidPersistenceContext, KdmidRequestCache> _writer = writer;
    private readonly MongoDbReaderRepository<KdmidPersistenceContext, KdmidRequestCache> _reader = reader;

    public async Task SetSessionId(City city, KdmidId kdmidId, string sessionId, CancellationToken cToken)
    {
        Expression<Func<KdmidRequestCache, bool>> filter = x => x.City.Code == city.Code && x.KdmidId.Id == kdmidId.Id;

        var isCacheExist = await _reader.IsExists<KdmidRequestCache>(new(filter), cToken);

        if (isCacheExist)
        {
            await _writer.Update<KdmidRequestCache>(new(x => x.SessionId = sessionId)
            {
                QueryOptions = new(filter)
            }, cToken);
        }
        else
        {
            await _writer.CreateOne<KdmidRequestCache>(new()
            {
                City = city,
                KdmidId = kdmidId,
                SessionId = sessionId
            }, cToken);
        };
    }
    public async Task<string> GetSessionId(City city, KdmidId kdmidId, CancellationToken cToken)
    {
        Expression<Func<KdmidRequestCache, bool>> filter = x => x.City.Code == city.Code && x.KdmidId.Id == kdmidId.Id;

        var cache = await _reader.FindSingle<KdmidRequestCache>(new(filter), cToken);

        return string.IsNullOrWhiteSpace(cache?.SessionId)
            ? throw new InvalidOperationException($"SessionId for '{city.Name}' with '{kdmidId.Id}' was not found.")
            : cache.SessionId;
    }

    public async Task SetHeaders(City city, KdmidId kdmidId, Dictionary<string, string> headers, CancellationToken cToken)
    {
        Expression<Func<KdmidRequestCache, bool>> filter = x => x.City.Code == city.Code && x.KdmidId.Id == kdmidId.Id;

        await _writer.Update<KdmidRequestCache>(new(x => x.Headers = headers)
        {
            QueryOptions = new(filter)
        }, cToken);

    }
    public async Task<Dictionary<string, string>> GetHeaders(City city, KdmidId kdmidId, CancellationToken cToken)
    {
        Expression<Func<KdmidRequestCache, bool>> filter = x => x.City.Code == city.Code && x.KdmidId.Id == kdmidId.Id;

        var cache = await _reader.FindSingle<KdmidRequestCache>(new(filter), cToken);

        if (cache is null || cache.Headers.Count == 0)
            throw new InvalidOperationException($"Headers for '{city.Name}' with '{kdmidId.Id}' was not found.");

        return cache.Headers;
    }
}
