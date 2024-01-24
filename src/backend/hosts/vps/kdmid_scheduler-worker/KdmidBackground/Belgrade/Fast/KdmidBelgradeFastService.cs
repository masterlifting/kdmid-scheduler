using KdmidScheduler.Abstractions.Interfaces.Core.Services;

using Microsoft.Extensions.Options;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Background.Abstractions.Models;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;
using Net.Shared.Background.Abstractions.Models.Settings;

namespace KdmidScheduler.Worker.KdmidBackground.Belgrade.Fast;

public sealed class KdmidBelgradeFastService(
    ILogger<KdmidBelgradeFastService> logger,
    IServiceScopeFactory scopeFactory,
    IBackgroundSettingsProvider settingsProvider
    ) : Net.Shared.Background.BackgroundService(KdmidBelgradeFastTask.Name, settingsProvider, logger)
{
    protected override async Task Start(BackgroundTask task, CancellationToken cToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptions<BackgroundTaskSettings>>();
        var repository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();
        var kdmidResponseService = scope.ServiceProvider.GetRequiredService<IKdmidResponseService>();

        var kdmidBelgradeFastTask = new KdmidBelgradeFastTask(logger, options, repository, kdmidResponseService);

        await kdmidBelgradeFastTask.Run(task, cToken);
    }
}
