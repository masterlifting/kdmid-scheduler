using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Microsoft.Extensions.Options;

using Net.Shared.Abstractions.Models.Settings;
using Net.Shared.Background;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Extensions.Logging;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;

using static KdmidScheduler.Abstractions.Constants;

namespace KdmidScheduler.Worker.KdmidBackground.Tasks;

public sealed class KdmidBelgradeTask(
    ILogger<KdmidBelgradeTask> logger,
    IOptions<CorrelationSettings> correlationOptions,
    IBackgroundSettingsProvider settingsProvider,
    IServiceScopeFactory serviceScopeFactory
    ) : BackgroundTask<KdmidAvailableDates>(Name, settingsProvider, logger)
{
    public const string Name = "Belgrade";
    private readonly ILogger<KdmidBelgradeTask> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly Guid _correlationId = correlationOptions.Value.Id;

    protected override IBackgroundTaskStepHandler<KdmidAvailableDates> GetStepHandler()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var kdmidResponseService = scope.ServiceProvider.GetRequiredService<IKdmidResponseService>();
        return new KdmidTaskStepHandler(kdmidResponseService);
    }
    protected override async Task<IPersistentProcessStep[]> GetSteps(CancellationToken cToken)
    {
        return [new KdmidAvailableDatesSteps
        {
            Id = (int)KdmidProcessSteps.CheckAvailableDates,
            Name = nameof(KdmidProcessSteps.CheckAvailableDates)
        }];

        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var processRepository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();
        return await processRepository.GetProcessSteps<KdmidAvailableDatesSteps>(cToken);
    }
    protected override async Task<KdmidAvailableDates[]> GetProcessableData(IPersistentProcessStep step, int limit, CancellationToken cToken)
    {
        _logger.Warn(nameof(GetProcessableData));
        return [];

        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var processRepository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();

        var data = await processRepository.GetProcessableData<KdmidAvailableDates>(_correlationId, step, limit, cToken);

        return KdmidTaskStepHandler.Filter(data, "belgrad");
    }
    protected override async Task<KdmidAvailableDates[]> GetUnprocessedData(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken)
    {
        _logger.Warn(nameof(GetUnprocessedData));
        return [];

        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var processRepository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();

        var data = await processRepository.GetUnprocessedData<KdmidAvailableDates>(_correlationId, step, limit, updateTime, maxAttempts, cToken);

        return KdmidTaskStepHandler.Filter(data, "belgrad");
    }
    protected override async Task SaveData(IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<KdmidAvailableDates> data, CancellationToken cToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var processRepository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();

        await processRepository.SetProcessedData(_correlationId, currentStep, nextStep, data, cToken);
    }
}
