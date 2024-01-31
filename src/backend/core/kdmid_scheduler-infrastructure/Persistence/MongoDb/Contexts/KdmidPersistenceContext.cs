﻿using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Net.Shared.Persistence.Abstractions.Models.Settings.Connections;
using Net.Shared.Persistence.Contexts;

using static KdmidScheduler.Abstractions.Constants;

namespace KdmidScheduler.Infrastructure.Persistence.MongoDb.Contexts;

public sealed class KdmidPersistenceContext(ILogger<KdmidPersistenceContext> logger, IOptions<MongoDbConnectionSettings> options) : MongoDbContext(logger, options.Value)
{
    public override void OnModelCreating(MongoDbBuilder builder)
    {
        builder.SetCollection<KdmidRequestCache>();
        builder.SetCollection<KdmidBotCommands>();
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
