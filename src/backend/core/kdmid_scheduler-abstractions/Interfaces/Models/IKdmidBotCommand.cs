using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Abstractions.Interfaces.Models;

public interface IKdmidBotCommand
{
    string ChatId { get; init; }
    BotCommand Command { get; set; }
}
