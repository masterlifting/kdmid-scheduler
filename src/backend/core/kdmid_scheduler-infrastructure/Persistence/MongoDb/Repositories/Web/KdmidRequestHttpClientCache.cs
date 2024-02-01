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

    static Expression<Func<KdmidRequestCache, bool>> Filter(City city, KdmidId kdmidId) => x => x.City.Code == city.Code && x.KdmidId.Id == kdmidId.Id;

    public async Task SetSessionId(City city, KdmidId kdmidId, string sessionId, ushort sec, CancellationToken cToken)
    {
        var now = DateTime.UtcNow;

        var isCacheExist = await _reader.IsExists<KdmidRequestCache>(new(Filter(city, kdmidId)), cToken);

        if (isCacheExist)
        {
            await _writer.Update<KdmidRequestCache>(new(x =>
            {
                x.Updated = now;
                x.SessionId = sessionId;
                x.SessionExpires = now.AddSeconds(sec);
            })
            {
                QueryOptions = new(Filter(city, kdmidId))
            }, cToken);
        }
        else
        {
            await _writer.CreateOne<KdmidRequestCache>(new()
            {
                Updated = now,

                City = city,
                KdmidId = kdmidId,
                SessionId = sessionId,
                SessionExpires = now.AddSeconds(sec),
            }, cToken);
        };
    }
    public async Task<string> GetSessionId(City city, KdmidId kdmidId, CancellationToken cToken)
    {
        var cache = await _reader.FindMany<KdmidRequestCache>(new(x => x.City.Code == city.Code), cToken);

        if (cache.Length == 0)
            throw new InvalidOperationException($"SessionId for '{city.Name}' was not found.");

        var cacheItem = cache.MinBy(x => x.SessionExpires);

        if (cacheItem!.SessionExpires < DateTime.UtcNow)
            throw new InvalidOperationException($"SessionId for '{city.Name}' was expired.");

        return cacheItem.SessionId;
    }

    public async Task SetHeaders(City city, KdmidId kdmidId, Dictionary<string, string> headers, CancellationToken cToken)
    {
        await _writer.Update<KdmidRequestCache>(new(x => x.Headers = headers)
        {
            QueryOptions = new(Filter(city, kdmidId))
        }, cToken);

    }
    public async Task<Dictionary<string, string>> GetHeaders(City city, KdmidId kdmidId, CancellationToken cToken)
    {
        var cache = await _reader.FindSingle<KdmidRequestCache>(new(Filter(city, kdmidId)), cToken);

        if (cache is null || cache.Headers.Count == 0)
            throw new InvalidOperationException($"Headers for '{city.Name}' with '{kdmidId.Id}' was not found.");

        return cache.Headers;
    }

    public async Task Clear(City city, KdmidId kdmidId, CancellationToken cToken)
    {
        _ = await _writer.Delete<KdmidRequestCache>(new(Filter(city, kdmidId)), cToken);
    }
}
