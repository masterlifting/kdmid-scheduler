using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace AP.Telegram.Notifier
{
    public class Notifier
    {
        [FunctionName("aptgnotifier")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            var token = Environment.GetEnvironmentVariable("AptgnotifierToken", EnvironmentVariableTarget.Process);
            var chatId = Environment.GetEnvironmentVariable("AptgnotifierChatId", EnvironmentVariableTarget.Process);

            var telegram = new TelegramBotClient(token);
            
            try
            {
                await telegram.SendTextMessageAsync(long.Parse(chatId), "Hello World");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error sending message");
            }
        }
    }
}
