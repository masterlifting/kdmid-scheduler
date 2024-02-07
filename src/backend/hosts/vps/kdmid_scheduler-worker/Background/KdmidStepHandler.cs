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
                    var logger = serviceProvider.GetRequiredService<ILogger<KdmidStepHandler>>();

                    return Task.WhenAll(data.Select(x =>
                    {
                        try
                        {
                            return service.SendAvailableDates(new(null, x.Chat), x.Command, cToken);
                        }
                        catch (Exception exception)
                        {
                            logger.Error($"Available dates for the task '{taskName}' were failed for the chat '{x.Chat}'. Reason: {exception.Message}");

                            x.Error = exception.Message;
                            x.StatusId = (int)ProcessStatuses.Error;

                            return Task.CompletedTask;
                        }
                    }));
                }
            default:
                throw new NotSupportedException($"Step '{step.Name}' of the task '{taskName}' is not supported.");
        }
    }
}
