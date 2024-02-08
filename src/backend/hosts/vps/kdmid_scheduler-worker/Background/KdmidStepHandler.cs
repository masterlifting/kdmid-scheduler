using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Extensions.Logging;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;

using static KdmidScheduler.Abstractions.Constants;
using static Net.Shared.Persistence.Abstractions.Constants.Enums;

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

                    return Task.WhenAll(data.Select(async x =>
                    {
                        try
                        {
                            await service.SendAvailableDates(new(null, x.Chat), x.Command, cToken);
                        }
                        catch (Exception exception)
                        {
                            var logger = serviceProvider.GetRequiredService<ILogger<KdmidStepHandler>>();

                            logger.Error($"Available dates for the task '{taskName}' were failed for the chat '{x.Chat}'. Reason: {exception.Message}");

                            x.Error = exception.Message;
                            x.StatusId = (int)ProcessStatuses.Error;
                        }
                    }));
                }
            default:
                throw new NotSupportedException($"Step '{step.Name}' of the task '{taskName}' is not supported.");
        }
    }
}
