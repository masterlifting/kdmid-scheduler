using System;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Telegram.ApAzureBot.Core.Abstractions;
using Telegram.ApAzureBot.Infrastructure.Abstractions;

namespace Telegram.ApAzureBot.Worker
{
    public class Functions
    {
        private readonly ILogger _logger;
        private readonly IResponseService _responseService;

        public Functions(ILoggerFactory loggerFactory, IResponseService responseService)
        {
            _logger = loggerFactory.CreateLogger<Functions>();
            _responseService = responseService;
        }

        [Function("TelegramApAzureBotWorker")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] MyInfo myTimer, CancellationToken token)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");

            await _responseService.Process(0,"", token);
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
