using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Microsoft.Extensions.Options;

using Net.Shared.Abstractions.Models.Settings;
using Net.Shared.Background;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;

namespace KdmidScheduler.Worker.KdmidBackground.Tasks;

public sealed class KdmidParisTask(
    ILogger<KdmidParisTask> logger,
    IOptions<HostSettings> hostOptions,
    IBackgroundSettingsProvider settingsProvider,
    IServiceScopeFactory serviceScopeFactory
    ) : BackgroundTask<KdmidAvailableDates>("Paris", settingsProvider, logger)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly Guid _hostId = hostOptions.Value.Id;

    protected override IBackgroundTaskStepHandler<KdmidAvailableDates> GetStepHandler()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var kdmidResponseService = scope.ServiceProvider.GetRequiredService<IKdmidResponseService>();
        return new KdmidTaskStepHandler(kdmidResponseService);
    }
    protected override async Task<IPersistentProcessStep[]> GetSteps(CancellationToken cToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var processRepository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();
        return await processRepository.GetProcessSteps<KdmidAvailableDatesSteps>(cToken);
    }
    protected override async Task<KdmidAvailableDates[]> GetProcessableData(IPersistentProcessStep step, int limit, CancellationToken cToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var processRepository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();

        var data = await processRepository.GetProcessableData<KdmidAvailableDates>(_hostId, step, limit, cToken);

        return KdmidTaskStepHandler.Filter(data, "paris");
    }
    protected override async Task<KdmidAvailableDates[]> GetUnprocessedData(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var processRepository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();

        var data = await processRepository.GetUnprocessedData<KdmidAvailableDates>(_hostId, step, limit, updateTime, maxAttempts, cToken);

        return KdmidTaskStepHandler.Filter(data, "paris");
    }
    protected override async Task SaveData(IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<KdmidAvailableDates> data, CancellationToken cToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var processRepository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();

        await processRepository.SetProcessedData(_hostId, currentStep, nextStep, data, cToken);
    }
}
