using Azure;
using Azure.Data.Tables;

using Net.Shared.Persistence.Abstractions.Entities;

namespace Telegram.ApAzureBot.Core.Persistence.NoSql;

public sealed class TelegramCommandTask : IPersistentNoSql, IPersistentProcess, ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public long ChatId { get; init; }
    public string Text { get; init; } = null!;
    public string DocumentVersion { get; set; }
    public DateTime Created { get; set; }
    public string? Description { get; set; }
    public Guid? HostId { get; set; }
    public int StatusId { get; set; }
    public int StepId { get; set; }
    public byte Attempt { get; set; }
    public string? Error { get; set; }
    public DateTime Updated { get; set; }
}
