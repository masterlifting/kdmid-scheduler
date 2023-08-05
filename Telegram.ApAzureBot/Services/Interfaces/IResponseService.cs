using Telegram.Bot.Types;

namespace Telegram.ApAzureBot.Services.Interfaces;

public interface IResponseService
{
    Task Process(Message message, CancellationToken cToken);
    Task SetResponse(string message);
}
