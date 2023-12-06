using Azure;
using Azure.Data.Tables;

using Net.Shared.Persistence.Abstractions.Entities;

namespace Telegram.ApAzureBot.Core.Persistence.Entities;

public sealed class TelegramCommandTask : IPersistentProcess, ITableEntity
{
    public long ChatId { get; init; }
    public string Text { get; init; } = null!;

    public Guid? HostId { get; set; }
    public int StatusId { get; set; }
    public int StepId { get; set; }
    public int Attempt { get; set; }
    public string? Error { get; set; }

    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = null!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public DateTime Updated { get; set; } = DateTime.UtcNow;
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public string? Description { get; set; }
}
