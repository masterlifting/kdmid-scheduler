using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Entities;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

using MongoDB.Bson;

using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

public sealed class KdmidRequestCache : IKdmidRequestCache, IPersistentNoSql
{
    public ObjectId Id { get; set; } = ObjectId.Empty;
    
    public City City { get; set; } = null!;
    public KdmidId KdmidId { get; set; } = null!;

    public string SessionId { get; set; } = null!;
    public Dictionary<string, string> Headers { get; set; } = [];

    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    public string? Description { get; set; }
    public string DocumentVersion { get; set; } = "1.0.0";
}
