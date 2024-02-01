using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Entities;

public interface IKdmidRequestCache : IPersistent
{
    City City { get; set; }
    KdmidId KdmidId { get; set; } 
    
    string SessionId { get; set; }
    DateTime SessionExpires { get; set; }
    
    Dictionary<string, string> Headers { get; set; }

    DateTime Updated { get; set; }
}
