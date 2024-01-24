using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;

using static KdmidScheduler.Abstractions.Constants;

namespace KdmidScheduler.Worker.KdmidBackground;

public sealed class KdmidTaskStepHandler(IKdmidResponseService kdmidResponseService) : IBackgroundTaskStepHandler<KdmidAvailableDates>
{
    private readonly IKdmidResponseService _kdmidResponseService = kdmidResponseService;

    public async Task<Result<KdmidAvailableDates>> Handle(string taskName, IPersistentProcessStep step, IEnumerable<KdmidAvailableDates> data, CancellationToken cToken)
    {
        switch (step.Id)
        {
            case (int)KdmidProcessSteps.CheckAvailableDates:
                {
                    foreach (var item in data)
                    {
                        await _kdmidResponseService.SendAvailableDates(item.Chat, item.Command, cToken);
                    }

                    return new(data);
                }
            default:
                throw new NotSupportedException($"Step '{step.Name}' of the task '{taskName}' is not supported.");
        }
    }
}
