using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Net.Shared.Extensions.Logging;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;

using static KdmidScheduler.Abstractions.Constants;
using static Net.Shared.Persistence.Abstractions.Constants.Enums;

namespace KdmidScheduler.Worker.KdmidBackground;

public sealed class KdmidBackgroundStepHandler : IBackgroundTaskStepHandler<KdmidAvailableDates>
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
                        try
                        {
                            await kdmidResponseService.SendAvailableDates(item.Chat, item.Command, cToken);
                        }
                        catch (Exception exception)
                        {
                            item.StatusId = (int)ProcessStatuses.Error;
                            item.Error = exception.Message;

                            var logger = serviceProvider.GetRequiredService<ILogger<KdmidBackgroundStepHandler>>();
                            logger.Error($"Handling step '{step.Name}' for the '{taskName}' has failed. Reason: {exception.Message}.");
                        }
                    }
                }
                break;
            default:
                throw new NotSupportedException($"Step '{step.Name}' of the task '{taskName}' is not supported.");
        }
    }
}
