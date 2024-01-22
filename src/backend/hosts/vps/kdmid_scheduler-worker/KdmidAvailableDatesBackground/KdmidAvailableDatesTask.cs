using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Microsoft.Extensions.Options;

using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Background.Abstractions.Models.Settings;
using Net.Shared.Background.Core;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;

namespace KdmidScheduler.Worker.KdmidAvailableDatesBackground;

public sealed class KdmidAvailableDatesTask(
    ILogger logger,
    IOptions<BackgroundTaskSettings> options,
    IPersistenceNoSqlProcessRepository processRepository,
    IKdmidResponseService kdmidResponseService
    ) : BackgroundTask<KdmidAvailableDates>(logger)
{
    public const string Name = "GetAvailableDates";
    private readonly BackgroundTaskSettings _backgroundTaskSettings = options.Value;

    protected override async Task<IPersistentProcessStep[]> GetSteps(CancellationToken cToken) =>
        await processRepository.GetProcessSteps<KdmidAvailableDatesSteps>(cToken);
    protected override IBackgroundTaskStep<KdmidAvailableDates> RegisterStepHandler() => 
        new KdmidAvailableDatesStepHandler(
            logger,
            kdmidResponseService);
    protected override Task<KdmidAvailableDates[]> GetProcessableData(IPersistentProcessStep step, int limit, CancellationToken cToken) =>
        processRepository.GetProcessableData<KdmidAvailableDates>(_backgroundTaskSettings.HostId, step, limit, cToken);
    protected override Task<KdmidAvailableDates[]> GetUnprocessedData(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) =>
        processRepository.GetUnprocessedData<KdmidAvailableDates>(_backgroundTaskSettings.HostId, step, limit, updateTime, maxAttempts, cToken);
    protected override Task SaveData(IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<KdmidAvailableDates> data, CancellationToken cToken) =>
        processRepository.SetProcessedData(_backgroundTaskSettings.HostId, currentStep, nextStep, data, cToken);
}
