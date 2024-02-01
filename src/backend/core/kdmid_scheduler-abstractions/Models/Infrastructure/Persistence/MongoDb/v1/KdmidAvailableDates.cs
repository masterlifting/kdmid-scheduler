using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Bots.Abstractions.Models.Bot;
using MongoDB.Bson;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Entities;

namespace KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

public sealed class KdmidAvailableDates : IKdmidRequestAvailableDates, IPersistentNoSql
{
    public ObjectId Id { get; set; } = ObjectId.Empty;
    public Guid? CorrelationId { get; set; }

    public City City { get; set; } = null!;
    public Chat Chat { get; set; } = null!;
    public Command Command { get; set; } = null!;

    public int StatusId { get; set; }
    public int StepId { get; set; }
    public int Attempt { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;

    public string? Error { get; set; }
    public string? Description { get; set; }
    public string DocumentVersion { get; set; } = "1.0.0";
}
