using Telegram.ApAzureBot.Core.Abstractions;

namespace Telegram.ApAzureBot.Infrastructure.Abstractions
{
    public interface IKdmidService : IProcessService
    {
        Task Schedule(long chatId, string city, string parameters, CancellationToken cToken);
        Task Captcha(long chatId, string city, string parameters, CancellationToken cToken);
        Task Confirm(long chatId, string city, string parameters, CancellationToken cToken);
    }
}
