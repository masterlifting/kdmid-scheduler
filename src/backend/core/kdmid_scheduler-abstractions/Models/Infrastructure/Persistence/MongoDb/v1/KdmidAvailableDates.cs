using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Bots.Abstractions.Models.Bot;
using MongoDB.Bson;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

namespace KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

public sealed class KdmidAvailableDates : IPersistentNoSql, IPersistentProcess
{
    public ObjectId Id { get; set; } = ObjectId.Empty;
    public Guid? CorrelationId { get; set; }
    public string DocumentVersion { get; set; } = "1.0.0";

    public City City { get; set; } = null!;
    public Chat Chat { get; set; } = null!;
    public Command Command { get; set; } = null!;

    public int StatusId { get; set; }
    public int StepId { get; set; }
    public int Attempt { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; }

    public string? Error { get; set; }
    public string? Description { get; set; }
}
