using System.Linq.Expressions;

using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;
using KdmidScheduler.Infrastructure.Persistence.Contexts;

using Microsoft.Extensions.Options;

using Net.Shared.Abstractions.Models.Settings;
using Net.Shared.Background;
using Net.Shared.Background.Abstractions.Interfaces;

namespace KdmidScheduler.Worker.KdmidBackground.Tasks;

public sealed class KdmidBelgradeTask(
    ILogger<KdmidBelgradeTask> logger,
    IOptions<CorrelationSettings> correlationOptions,
    IServiceScopeFactory serviceScopeFactory,
    IBackgroundSettingsProvider settingsProvider
    ) : BackgroundTask<
            KdmidTaskStepHandler, 
            KdmidAvailableDates,
            KdmidAvailableDatesSteps,
            KdmidMongoDbContext>
    (Name, correlationOptions.Value.Id,  logger, serviceScopeFactory, settingsProvider)
{
    public const string Name = "Belgrade";

    protected override Expression<Func<KdmidAvailableDates, bool>> DataFilter => x => KdmidTaskStepHandler.Filter(x, "belgrad");
}
