using Azure.Data.Tables;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Azure;
using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Models.Persistence;
using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.AzureTable.v1;

public sealed class KdmidBotCommand : IKdmidBotCommand, IPersistent, ITableEntity
{
    public string ChatId { get; init; } = null!;
    public Command Command { get; set; } = null!;
    public DateTime Created { get; set; }
    public string? Description { get; set; }
    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = null!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
