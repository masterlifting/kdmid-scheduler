using Telegram.Bot;
using Telegram.Bot.Types;

namespace Telegram.ApAzureBot.Services.Interfaces;

public interface IResponseService
{
    Task SetResponse(TelegramBotClient bot, Message message);
}
