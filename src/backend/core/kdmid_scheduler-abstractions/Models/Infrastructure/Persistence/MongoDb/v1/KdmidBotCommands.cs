﻿using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Entities;
using MongoDB.Bson;

using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

public sealed class KdmidBotCommands : IKdmidBotCommands, IPersistentNoSql
{
    public ObjectId Id { get; set; } = ObjectId.Empty;
    
    public string ChatId { get; init; } = null!;
    public Command Command { get; set; } = null!;
    
    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    public string? Description { get; set; }
    public string DocumentVersion { get; set; } = "1.0.0";
}
