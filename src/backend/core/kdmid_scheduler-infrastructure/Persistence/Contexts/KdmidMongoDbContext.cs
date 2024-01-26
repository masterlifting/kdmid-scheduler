using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Microsoft.Extensions.Options;

using Net.Shared.Persistence.Abstractions.Models.Settings.Connections;
using Net.Shared.Persistence.Contexts;

using static KdmidScheduler.Abstractions.Constants;

namespace KdmidScheduler.Infrastructure.Persistence.Contexts;

public sealed class KdmidMongoDbContext(IOptions<MongoDbConnectionSettings> options) : MongoDbContext(options.Value)
{
    public override void OnModelCreating(MongoDbBuilder builder)
    {
        builder.SetCollection<Abstractions.Models.Infrastructure.Persistence.MongoDb.v1.KdmidBotCommands>();
        builder.SetCollection<KdmidAvailableDates>();
        builder.SetCollection(new KdmidAvailableDatesSteps[]
        {
            new()
            {
                Id = (int)KdmidProcessSteps.CheckAvailableDates,
                Name = nameof(KdmidProcessSteps.CheckAvailableDates)
            }
        });
    }
}
