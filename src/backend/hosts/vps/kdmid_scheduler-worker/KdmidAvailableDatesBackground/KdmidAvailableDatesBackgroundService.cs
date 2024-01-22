using KdmidScheduler.Abstractions.Interfaces.Core.Services;

using Microsoft.Extensions.Options;

using Net.Shared.Extensions.Logging;
using Net.Shared.Abstractions.Models.Settings.Connection;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Background.Abstractions.Models;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;
using Net.Shared.Background.Abstractions.Models.Settings;

namespace KdmidScheduler.Worker.KdmidAvailableDatesBackground;

public sealed class KdmidAvailableDatesBackgroundService(
    IServiceScopeFactory scopeFactory,
    IBackgroundServiceConfigurationProvider configurationProvider,
    ILogger<KdmidAvailableDatesBackgroundService> logger) : Net.Shared.Background.Core.BackgroundService(KdmidAvailableDatesTask.Name, configurationProvider, logger)
{
    protected override async Task Run(BackgroundTaskModel taskModel, CancellationToken cToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var processRepository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();
        var backgroundTaskOptions = scope.ServiceProvider.GetRequiredService<IOptions<BackgroundTaskSettings>>();
        var kdmidResponseService = scope.ServiceProvider.GetRequiredService<IKdmidResponseService>();

        var task = new KdmidAvailableDatesTask(
            logger,
            backgroundTaskOptions,
            processRepository,
            kdmidResponseService
            );

        await task.Run(taskModel, cToken);
    }
}
