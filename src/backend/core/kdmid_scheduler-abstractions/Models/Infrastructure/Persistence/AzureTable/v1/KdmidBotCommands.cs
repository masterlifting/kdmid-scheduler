using Azure.Data.Tables;
using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Entities;
using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.AzureTable.v1;

public sealed class KdmidBotCommands : IKdmidBotCommands, ITableEntity
{
    public string ChatId { get; init; } = null!;
    public Command Command { get; set; } = null!;
    public DateTime Created { get; set; }
    public string? Description { get; set; }
    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = null!;
    public DateTimeOffset? Timestamp { get; set; }
    public Azure.ETag ETag { get; set; }
}
