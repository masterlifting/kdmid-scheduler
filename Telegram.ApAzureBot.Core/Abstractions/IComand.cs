using Telegram.Bot.Types;

namespace Telegram.ApAzureBot.Core.Abstractions
{
    public interface IResponseService
    {
        Task Process(Message message, CancellationToken cToken);
    }
}
