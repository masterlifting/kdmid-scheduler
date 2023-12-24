using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;

namespace TelegramBot.Abstractions.Models.Persistence.Entities;

public sealed class TelegramCommandStep : IPersistentProcessStep
{
    public int Id { get; init; }
    public string Name { get; set; } = null!;
    public DateTime Created { get; set; }
    public string? Description { get; set; }
    public string DocumentVersion { get; set; } = "1.0.0";
}
