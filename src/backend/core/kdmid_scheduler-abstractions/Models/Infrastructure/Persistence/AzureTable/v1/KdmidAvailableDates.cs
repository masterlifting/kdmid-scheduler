using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Entities;
using Azure.Data.Tables;
using Azure;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.AzureTable.v1;

public sealed class KdmidAvailableDates : IKdmidRequestAvailableDates, ITableEntity
{
    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = null!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public City City { get; set; } = null!;
    public Chat Chat { get; set; } = null!;
    public Command Command { get; set; } = null!;
    public Guid? CorrelationId { get; set; }
    public int StatusId { get; set; }
    public int StepId { get; set; }
    public int Attempt { get; set; }
    public string? Error { get; set; }
    public DateTime Updated { get; set; }
    public DateTime Created { get; set; }
    public string? Description { get; set; }
}
