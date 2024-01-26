using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Extensions.Serialization.Json;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;

using static KdmidScheduler.Abstractions.Constants;
using static Net.Shared.Persistence.Abstractions.Constants.Enums;

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
                        try
                        {
                            await _kdmidResponseService.SendAvailableDates(item.Chat, item.Command, cToken);
                        }
                        catch (Exception exception)
                        {
                            item.StatusId = (int)ProcessStatuses.Error;
                            item.Error = exception.Message;
                        }
                    }

                    return new(data);
                }
            default:
                throw new NotSupportedException($"Step '{step.Name}' of the task '{taskName}' is not supported.");
        }
    }

    public static KdmidAvailableDates[] Filter(IEnumerable<KdmidAvailableDates> data, string cityCode) =>
        data.Where(x =>
        {
            if (x.Command.Parameters.TryGetValue(BotCommandParametersCityKey, out var cityStr))
            {
                var city = cityStr.FromJson<City>();

                return city.Code == cityCode;
            }
            else
                return false;
        }).ToArray();
}
