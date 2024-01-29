using System.Linq.Expressions;

using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Microsoft.Extensions.Options;

using Net.Shared.Abstractions.Models.Settings;
using Net.Shared.Background;
using Net.Shared.Background.Abstractions.Interfaces;

namespace KdmidScheduler.Worker.KdmidBackground.Tasks;

public sealed class KdmidTiranaTask(
    ILogger<KdmidTiranaTask> logger,
    IOptions<CorrelationSettings> correlationOptions,
    IServiceScopeFactory serviceScopeFactory,
    IBackgroundSettingsProvider settingsProvider
    ) : BackgroundTask<
            KdmidAvailableDates,
            KdmidAvailableDatesSteps,
            KdmidBackgroundStepHandler>
    (Name, correlationOptions.Value.Id, logger, serviceScopeFactory, settingsProvider)
{
    public const string Name = "Tirana";

    protected override Expression<Func<KdmidAvailableDates, bool>> DataFilter => x => x.City.Code == "tirana";
}
