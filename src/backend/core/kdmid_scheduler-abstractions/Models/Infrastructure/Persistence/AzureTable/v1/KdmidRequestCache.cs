using Azure;
using Azure.Data.Tables;
using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Entities;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.AzureTable.v1;

public sealed class KdmidRequestCache : IKdmidRequestCache, IPersistent, ITableEntity
{
    public ETag ETag { get; set; }
    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = null!;
    public DateTimeOffset? Timestamp { get; set; }

    public City City { get; set; } = null!;
    public KdmidId KdmidId { get; set; } = null!;

    public string SessionId { get; set; } = null!;
    public DateTime SessionExpires { get; set; }

    public Dictionary<string, string> Headers { get; set; } = [];

    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;

    public string? Description { get; set; }
    public string DocumentVersion { get; set; } = "1.0.0";
}
