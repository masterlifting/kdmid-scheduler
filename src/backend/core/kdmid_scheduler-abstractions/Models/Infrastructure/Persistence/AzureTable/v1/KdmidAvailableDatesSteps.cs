using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Models.Entities.Catalogs;
using Azure.Data.Tables;
using Azure;

namespace KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.AzureTable.v1;

public sealed class KdmidAvailableDatesSteps : PersistentCatalog, ITableEntity, IPersistentProcessStep
{
    public string DocumentVersion { get; set; } = "1.0.0";
    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = null!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
