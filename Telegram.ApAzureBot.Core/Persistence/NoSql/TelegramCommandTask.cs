using Net.Shared.Persistence.Abstractions.Entities;

namespace Telegram.ApAzureBot.Core.Persistence.NoSql;

public sealed class TelegramCommandTask : IPersistentNoSql, IPersistentProcess
{
    public TelegramMessage Message { get; init; } = null!;
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
