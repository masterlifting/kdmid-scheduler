using Telegram.Bot.Types;

namespace Telegram.ApAzureBot.Services.Interfaces
{
    public interface IResponseService
    {
        string Create(Update request);
    }
}
