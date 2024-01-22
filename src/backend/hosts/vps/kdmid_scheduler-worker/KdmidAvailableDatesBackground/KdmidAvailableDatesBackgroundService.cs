using KdmidScheduler.Abstractions.Interfaces.Core.Services;

using Microsoft.Extensions.Options;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Background.Abstractions.Models;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;
using Net.Shared.Background.Abstractions.Models.Settings;

namespace KdmidScheduler.Worker.KdmidAvailableDatesBackground;

public sealed class KdmidAvailableDatesBackgroundService(
    IServiceScopeFactory scopeFactory,
    IBackgroundServiceConfigurationProvider configurationProvider,
    ILogger<KdmidAvailableDatesBackgroundService> logger) : Net.Shared.Background.Core.BackgroundService(KdmidAvailableDatesTaskRunner.TaskName, configurationProvider, logger)
{
    protected override async Task Run(BackgroundTask task, CancellationToken cToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var processRepository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();
        var backgroundTaskOptions = scope.ServiceProvider.GetRequiredService<IOptions<BackgroundTaskSettings>>();
        var kdmidResponseService = scope.ServiceProvider.GetRequiredService<IKdmidResponseService>();

        var runner = new KdmidAvailableDatesTaskRunner(
            logger,
            backgroundTaskOptions,
            processRepository,
            kdmidResponseService);

        await runner.Run(task, cToken);
    }
}
