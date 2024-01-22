using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Net.Shared.Extensions.Logging;
using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;

using static KdmidScheduler.Abstractions.Constants;

namespace KdmidScheduler.Worker.KdmidAvailableDatesBackground;

public sealed class KdmidAvailableDatesStepHandler(
    ILogger logger,
    IKdmidResponseService kdmidResponseService) : IBackgroundTaskStepHandler<KdmidAvailableDates>
{
    private readonly ILogger _logger = logger;
    private readonly IKdmidResponseService _kdmidResponseService = kdmidResponseService;

    public async Task<Result<KdmidAvailableDates>> Handle(
        IPersistentProcessStep step,
        IEnumerable<KdmidAvailableDates> data,
        CancellationToken cToken)
    {
        switch (step.Id)
        {
            case (int)KdmidProcessSteps.CheckAvailableDates:
                {
                    const int MaxAvailableMinutesBetweenRequests = 20 - 3;
                    var dataCount = data.Count();

                    var periodMinutes = MaxAvailableMinutesBetweenRequests / dataCount;

                    var period = TimeSpan.FromMinutes(periodMinutes);

                    using var timer = new PeriodicTimer(period);

                    foreach (var item in data)
                    {
                        try
                        {
                            await _kdmidResponseService.SendAvailableDates(item.Chat, item.Command, cToken);
                        }
                        catch (Exception exception)
                        {
                            _logger.ErrorCompact(exception);
                        }
                        finally
                        {
                            await timer.WaitForNextTickAsync(cToken);
                        }
                    }

                    return new(data);
                }
            default:
                throw new NotSupportedException($"The step '{step.Name}' of the task '{KdmidAvailableDatesTaskRunner.TaskName}' was not recognized.");
        }
    }
}
