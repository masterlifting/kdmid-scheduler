using KdmidScheduler.Worker.Models;

using Microsoft.Azure.Functions.Worker;

namespace KdmidScheduler.Worker;

//public class Functions(ITelegramCommandTaskService service)
//{
//    private readonly ITelegramCommandTaskService _service = service;

//    private const string FastSeekFunction = "fast-seek";
//    private const string SlowSeekFunction = "slow-seek";

//    [Function(FastSeekFunction)]
//    public async Task RunFastSeek([TimerTrigger("15,40 6-7 * * 1-5")] TelegramTimer timer) => await _service.Process(
//    [
//        Constants.Kdmid.Cities.Budapest,
//        Constants.Kdmid.Cities.Belgrade
//    ]);

//    [Function(SlowSeekFunction)]
//    public async Task RunSlowSeek([TimerTrigger("0/30 7-14 * * 1-5")] TelegramTimer timer) => await _service.Process([]);
//}
