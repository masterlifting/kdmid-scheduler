using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Models.Persistence;

public interface IKdmidBotCommand
{
    string ChatId { get; init; }
    BotCommand Command { get; set; }
}
