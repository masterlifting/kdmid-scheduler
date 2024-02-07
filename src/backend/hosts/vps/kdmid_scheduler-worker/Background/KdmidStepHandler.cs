using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;

using static KdmidScheduler.Abstractions.Constants;

namespace KdmidScheduler.Worker.Background;

public sealed class KdmidStepHandler : IBackgroundTaskStepHandler<KdmidAvailableDates>
{
    public Task Handle(
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
                    var service = serviceProvider.GetRequiredService<IKdmidResponseService>();

                    var serviceTasks = data.Select(x => service.SendAvailableDates(new(null, x.Chat), x.Command, cToken));
                    
                    return Task
                        .WhenAll(serviceTasks)
                        .ContinueWith(result => {
                            if (result.IsFaulted)
                            {
                                throw result.Exception;
                            }
                        }, cToken);
                }
            default:
                throw new NotSupportedException($"Step '{step.Name}' of the task '{taskName}' is not supported.");
        }
    }
}
