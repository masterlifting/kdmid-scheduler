using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Background.Core;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;

namespace KdmidScheduler.Worker.KdmidAvailableDates;

public sealed class KdmidAvailableDatesTask(ILogger logger) : BackgroundTask<KdmidAvailableDatesModel>(logger)
{
    protected override Task<IPersistentProcessStep[]> GetSteps(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    protected override IBackgroundTaskStep<KdmidAvailableDatesModel> RegisterStepHandler()
    {
        throw new NotImplementedException();
    }
    protected override Task<KdmidAvailableDatesModel[]> GetProcessableData(IPersistentProcessStep step, int limit, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    protected override Task<KdmidAvailableDatesModel[]> GetUnprocessedData(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    protected override Task SaveData(IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<KdmidAvailableDatesModel> data, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
