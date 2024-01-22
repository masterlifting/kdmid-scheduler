using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Models.Entities.Catalogs;

namespace KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

public sealed class KdmidAvailableDatesSteps : PersistentCatalog, IPersistentNoSql, IPersistentProcessStep
{
    public string DocumentVersion { get; set; } = "1.0.0";
}
