using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;

namespace Telegram.ApAzureBot.Core.Persistence.NoSql;

public sealed class TelegramCommandStep : IPersistentProcessStep, IPersistentNoSql
{
    public int Id { get; init; }
    public string Name { get; set; }
    public DateTime Created { get; set; }
    public string? Description { get; set; }
    public string DocumentVersion { get; set; }
}
