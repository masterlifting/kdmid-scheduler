using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Microsoft.Extensions.Options;

using Net.Shared.Background;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Background.Abstractions.Models.Settings;
using Net.Shared.Extensions.Serialization.Json;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;

namespace KdmidScheduler.Worker.KdmidBackground;

public sealed class KdmidBelgradeSlowTask(
    ILogger logger,
    IOptions<BackgroundTaskSettings> options,
    IPersistenceNoSqlProcessRepository processRepository,
    IKdmidResponseService kdmidResponseService
    ) : BackgroundTaskRunner<KdmidAvailableDates>(logger)
{
    public const string TaskName = "KdmidBelgradeSlow";

    private readonly BackgroundTaskSettings _settings = options.Value;
    private readonly IPersistenceNoSqlProcessRepository _processRepository = processRepository;

    protected override IBackgroundTaskStepHandler<KdmidAvailableDates> GetStepHandler() =>
        new KdmidTaskStepHandler(kdmidResponseService);
    protected override async Task<IPersistentProcessStep[]> GetSteps(CancellationToken cToken) =>
        await _processRepository.GetProcessSteps<KdmidAvailableDatesSteps>(cToken);
    protected override async Task<KdmidAvailableDates[]> GetProcessableData(IPersistentProcessStep step, int limit, CancellationToken cToken)
    {
        var data = await _processRepository.GetProcessableData<KdmidAvailableDates>(_settings.HostId, step, limit, cToken);

        return data.Where(x =>
        {
            if(x.Command.Parameters.TryGetValue(Abstractions.Constants.BotCommandParametersCityKey, out var cityStr))
            {
                var city = cityStr.FromJson<City>();

                return city.Code == "belgrad";
            }
            else
                return false;
        }).ToArray();
    }
    protected override async Task<KdmidAvailableDates[]> GetUnprocessedData(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken)
    {
        var data =  await _processRepository.GetUnprocessedData<KdmidAvailableDates>(_settings.HostId, step, limit, updateTime, maxAttempts, cToken);

        return data.Where(x =>
        {
            if(x.Command.Parameters.TryGetValue(Abstractions.Constants.BotCommandParametersCityKey, out var cityStr))
            {
                var city = cityStr.FromJson<City>();

                return city.Code == "belgrad";
            }
            else
                return false;
        }).ToArray();
    }
    protected override Task SaveData(IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<KdmidAvailableDates> data, CancellationToken cToken) =>
        _processRepository.SetProcessedData(_settings.HostId, currentStep, nextStep, data, cToken);
}
