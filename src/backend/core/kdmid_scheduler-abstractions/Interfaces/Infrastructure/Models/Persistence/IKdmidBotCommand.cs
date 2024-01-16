using Net.Shared.Bots.Abstractions.Models.Bot;

namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Models.Persistence;

public interface IKdmidBotCommand
{
    string ChatId { get; init; }
    Command Command { get; set; }
}
