using Net.Shared.Persistence.Abstractions.Entities.Catalogs;

namespace Telegram.ApAzureBot.Core.Persistence.Entities;

public sealed class TelegramCommandStep : IPersistentProcessStep
{
    public int Id { get; init; }
    public string Name { get; set; }
    public DateTime Created { get; set; }
    public string? Description { get; set; }
    public string DocumentVersion { get; set; }
}
