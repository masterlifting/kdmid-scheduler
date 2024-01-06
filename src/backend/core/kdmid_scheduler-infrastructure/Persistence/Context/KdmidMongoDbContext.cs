using KdmidScheduler.Abstractions.Models.v1.Persistence.MongoDb;

using Microsoft.Extensions.Options;

using Net.Shared.Persistence.Abstractions.Models.Settings.Connections;
using Net.Shared.Persistence.Contexts;

namespace KdmidScheduler.Infrastructure.Persistence.Context;

public sealed class KdmidMongoDbContext(IOptions<MongoDbConnectionSettings> options) : MongoDbContext(options.Value)
{
    public override void OnModelCreating(MongoDbBuilder builder)
    {
        builder.SetCollection<KdmidBotCommands>();
    }
}
