
using Net.Shared.Bots.Abstractions.Interfaces;

namespace TelegramBot.Abstractions.Interfaces.Services.Kdmid;

public interface IKdmidService
{
    Task HandleCommand(string chatId, IBotCommand command);
}
