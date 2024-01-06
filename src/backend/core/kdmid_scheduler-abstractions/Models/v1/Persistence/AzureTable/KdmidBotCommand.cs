using KdmidScheduler.Abstractions.Interfaces.Models;

using Azure.Data.Tables;

using Net.Shared.Bots.Abstractions.Models;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Azure;

namespace KdmidScheduler.Abstractions.Models.v1.Persistence.AzureTable;

public sealed class KdmidBotCommand : IKdmidBotCommand, IPersistent, ITableEntity
{
    public string ChatId { get; init; } = null!;
    public BotCommand Command { get; set; } = null!;
    public DateTime Created { get; set; }
    public string? Description { get; set; }
    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = null!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
