using Telegram.Bot.Types;

namespace Telegram.ApAzureBot.Services.Interfaces
{
    public interface IResponseService
    {
        Task<string> CheckMidRf(Update request);
    }
}
