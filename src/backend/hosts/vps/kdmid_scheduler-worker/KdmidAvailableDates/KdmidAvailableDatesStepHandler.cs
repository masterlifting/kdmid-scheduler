using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;

using static KdmidScheduler.Abstractions.Constants;

namespace KdmidScheduler.Worker.KdmidAvailableDates;

public sealed class KdmidAvailableDatesStepHandler : IBackgroundTaskStep<KdmidAvailableDatesModel>
{
    public Task<Result<KdmidAvailableDatesModel>> Handle(
        IPersistentProcessStep step, 
        IEnumerable<KdmidAvailableDatesModel> data, 
        CancellationToken cToken)
    {
        switch (step.Id)
        {
            case (int)ProcessSteps.ParseBcsCompanies:
                {
                    await _bcsCompaniesHandler.Handle(data, cToken);

                    return new(data);
                }
            case (int)ProcessSteps.ParseBcsTransactions:
                {
                    await _bcsTransactionsHandler.Handle(data, cToken);

                    return new(data);
                }
            case (int)ProcessSteps.ParseRaiffeisenSrbTransactions:
                {
                    await _raiffeisenSrbTransactionsHandler.Handle(data, cToken);

                    return new(data);
                }
            default:
                throw new NotImplementedException($"The step '{step.Name}' of the task '{DataHeapBackgroundTask.Name}' was not recognized.");
        }
    }
}
