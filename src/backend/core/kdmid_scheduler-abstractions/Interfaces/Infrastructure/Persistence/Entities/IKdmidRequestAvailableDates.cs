using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Entities;

public interface IKdmidRequestAvailableDates : IPersistentProcess
{
    City City { get; set; }
    Chat Chat { get; set; }
    Command Command { get; set; }
}
