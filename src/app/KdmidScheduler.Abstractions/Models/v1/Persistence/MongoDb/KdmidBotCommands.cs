using MongoDB.Bson;

using Net.Shared.Bots.Abstractions.Models;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace KdmidScheduler.Abstractions.Models.v1.Persistence.MongoDb;

public sealed class KdmidBotCommands : IPersistentNoSql
{
    public ObjectId Id { get; set; } = ObjectId.Empty;
    public string ChatId { get; set; } = null!;
    public Guid CommandId { get; set; }
    public BotCommand Command { get; set; } = default!;
    public string DocumentVersion { get; set; } = "1.0.0";
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
}
