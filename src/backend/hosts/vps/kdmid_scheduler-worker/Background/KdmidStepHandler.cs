using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;

using static KdmidScheduler.Abstractions.Constants;

namespace KdmidScheduler.Worker.Background;

public sealed class KdmidStepHandler : IBackgroundTaskStepHandler<KdmidAvailableDates>
{
    public async Task Handle(
        string taskName,
        IServiceProvider serviceProvider,
        IPersistentProcessStep step,
        IEnumerable<KdmidAvailableDates> data,
        CancellationToken cToken)
    {
        switch (step.Id)
        {
            case (int)KdmidProcessSteps.CheckAvailableDates:
                {
                    var kdmidResponseService = serviceProvider.GetRequiredService<IKdmidResponseService>();

                    foreach (var item in data)
                    {
                        await kdmidResponseService.SendAvailableDates(item.Chat, item.Command, cToken);
                    }
                }
                break;
            default:
                throw new NotSupportedException($"Step '{step.Name}' of the task '{taskName}' is not supported.");
        }
    }
}
