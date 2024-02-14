using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Net.Shared.Background.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Bot;
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
                        var message = new Message(null, x.Chat);

                        try
                        {
                            await service.SendAvailableDates(message, x.Command, cToken);
                        }
                        catch (Exception exception)
                        {
                            x.Error = exception.Message;
                            x.StatusId = (int)ProcessStatuses.Error;

                            var logger = serviceProvider.GetRequiredService<ILogger<KdmidStepHandler>>();
                            logger.Error($"Available dates for the task '{taskName}' were failed for the chat '{x.Chat.Id}'. Reason: {exception.Message}");
                            
                            var bot = serviceProvider.GetRequiredService<IBotClient>();
                            _ = await  bot.SendText(new(message, new($"Available dates for the task '{taskName}' were failed. Reason: {exception.Message}")), cToken);
                        }
                    }));
                }
            default:
                throw new NotSupportedException($"Step '{step.Name}' of the task '{taskName}' is not supported.");
        }
    }
}
