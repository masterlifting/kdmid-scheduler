using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Entities;

public interface IKdmidBotCommands : IPersistent
{
    string ChatId { get; init; }
    Command Command { get; set; }
}
