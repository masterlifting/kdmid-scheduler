using KdmidScheduler.Abstractions.Interfaces.Core.Services;

using Microsoft.Extensions.Options;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Background.Abstractions.Models;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;
using Net.Shared.Background.Abstractions.Models.Settings;

namespace KdmidScheduler.Worker.KdmidBackground;

public sealed class KdmidService(
    ILogger<KdmidService> logger,
    IServiceScopeFactory scopeFactory,
    IBackgroundSettingsProvider settingsProvider
    ) : Net.Shared.Background.BackgroundService(KdmidTaskRunner.TaskName, settingsProvider, logger)
{
    protected override async Task StartTask(BackgroundTask task, CancellationToken cToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptions<BackgroundTaskSettings>>();
        var repository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();
        var kdmidResponseService = scope.ServiceProvider.GetRequiredService<IKdmidResponseService>();

        var runner = new KdmidTaskRunner(logger, options, repository, kdmidResponseService);

        await runner.Run(task, cToken);
    }
}
