using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Repositories;

public interface IKdmidRequestHttpClientCache
{
    Task SetSessionId(City city, KdmidId kdmidId, string sessionId, ushort sec, CancellationToken cToken);
    Task<string> GetSessionId(City city, KdmidId kdmidId, CancellationToken cToken);

    Task<Dictionary<string, string>> GetHeaders(City city, KdmidId kdmidId, CancellationToken cToken);
    Task SetHeaders(City city, KdmidId kdmidId, Dictionary<string, string> headers, CancellationToken cToken);

    Task Clear(City city, KdmidId kdmidId, CancellationToken cToken);
}
