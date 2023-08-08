using Telegram.Bot.Types;

namespace Telegram.ApAzureBot.Infrastructure.Abstractions
{
    public interface IResponseService
    {
        Task Process(Message message, CancellationToken cToken);
    }
}
