using Azure;
using Azure.Data.Tables;

using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Models.Persistence;

using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.AzureTable.v1;

public sealed class KdmidRequestCache : IKdmidRequestCache, IPersistent, ITableEntity
{
    public DateTime Created { get; set; }
    public string? Description { get; set; }
    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = null!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
