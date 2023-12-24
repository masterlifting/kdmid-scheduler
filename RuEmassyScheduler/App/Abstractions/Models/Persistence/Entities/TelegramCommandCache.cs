using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace TelegramBot.Abstractions.Models.Persistence.Entities;

public sealed class TelegramCommandCache : IPersistent, ITableEntity
{
    public long ChatId { get; set; }
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;

    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = null!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
}
