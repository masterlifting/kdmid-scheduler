using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Net.Shared.Background;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Extensions.Serialization.Json;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;

namespace KdmidScheduler.Worker.KdmidBackground;

public sealed class KdmidBelgradeTask(
    ILogger<KdmidBelgradeTask> logger,
    IBackgroundSettingsProvider settingsProvider,
    IServiceScopeFactory serviceScopeFactory
    ) : BackgroundTask<KdmidAvailableDates>("KdmidBelgrade", settingsProvider, logger)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    
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
        
        var data = await processRepository.GetProcessableData<KdmidAvailableDates>(TaskSettings.HostId, step, limit, cToken);

        return data.Where(x =>
        {
            if (x.Command.Parameters.TryGetValue(Abstractions.Constants.BotCommandParametersCityKey, out var cityStr))
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
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var processRepository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();

        var data = await processRepository.GetUnprocessedData<KdmidAvailableDates>(TaskSettings.HostId, step, limit, updateTime, maxAttempts, cToken);

        return data.Where(x =>
        {
            if (x.Command.Parameters.TryGetValue(Abstractions.Constants.BotCommandParametersCityKey, out var cityStr))
            {
                var city = cityStr.FromJson<City>();

                return city.Code == "belgrad";
            }
            else
                return false;
        }).ToArray();
    }
    protected override async Task SaveData(IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<KdmidAvailableDates> data, CancellationToken cToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var processRepository = scope.ServiceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>();

        await processRepository.SetProcessedData(TaskSettings.HostId, currentStep, nextStep, data, cToken);
    }
}
