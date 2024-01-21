using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Background.Abstractions.Models;

namespace KdmidScheduler.Worker.KdmidAvailableDates;

public sealed class KdmidAvailableDatesBackgroundService(
    string taskName, 
    IBackgroundServiceConfigurationProvider provider, 
    ILogger<KdmidAvailableDatesBackgroundService> logger) : Net.Shared.Background.Core.BackgroundService(taskName, provider, logger)
{
    protected override Task Run(BackgroundTaskModel taskModel, CancellationToken cToken = default)
    {
        var task = new KdmidAvailableDatesTask(logger);

        return task.Run(taskModel, cToken);
    }
}
