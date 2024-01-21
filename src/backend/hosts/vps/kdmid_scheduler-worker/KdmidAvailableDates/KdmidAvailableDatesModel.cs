using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace KdmidScheduler.Worker.KdmidAvailableDates;

public sealed class KdmidAvailableDatesModel : IPersistentNoSql, IPersistentProcess
{
    public Guid? HostId { get; set; }
    public string DocumentVersion { get; set; } = "1.0.0";
    
    public Command? Command { get; init; }
    
    public int StatusId { get; set; }
    public int StepId { get; set; }
    public int Attempt { get; set; }
    
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    
    public string? Error { get; set; }
    public string? Description { get; set; }
}
