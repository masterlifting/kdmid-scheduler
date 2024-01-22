using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Microsoft.Extensions.Options;

using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Background.Abstractions.Models.Settings;
using Net.Shared.Background.Core;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;

namespace KdmidScheduler.Worker.KdmidAvailableDatesBackground;

public sealed class KdmidAvailableDatesTaskRunner(
    ILogger logger,
    IOptions<BackgroundTaskSettings> options,
    IPersistenceNoSqlProcessRepository processRepository,
    IKdmidResponseService kdmidResponseService
    ) : BackgroundTaskRunner<KdmidAvailableDates>(logger)
{
    public const string TaskName = "GetAvailableDates";

    private readonly ILogger _logger = logger;
    private readonly BackgroundTaskSettings _settings = options.Value;
    private readonly IPersistenceNoSqlProcessRepository _processRepository = processRepository;

    protected override IBackgroundTaskStepHandler<KdmidAvailableDates> RegisterStepHandler() =>
        new KdmidAvailableDatesStepHandler(
            _logger,
            kdmidResponseService);
    protected override async Task<IPersistentProcessStep[]> GetSteps(CancellationToken cToken) =>
        await _processRepository.GetProcessSteps<KdmidAvailableDatesSteps>(cToken);
    protected override Task<KdmidAvailableDates[]> GetProcessableData(IPersistentProcessStep step, int limit, CancellationToken cToken) =>
        _processRepository.GetProcessableData<KdmidAvailableDates>(_settings.HostId, step, limit, cToken);
    protected override Task<KdmidAvailableDates[]> GetUnprocessedData(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) =>
        _processRepository.GetUnprocessedData<KdmidAvailableDates>(_settings.HostId, step, limit, updateTime, maxAttempts, cToken);
    protected override Task SaveData(IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<KdmidAvailableDates> data, CancellationToken cToken) =>
        _processRepository.SetProcessedData(_settings.HostId, currentStep, nextStep, data, cToken);
}
