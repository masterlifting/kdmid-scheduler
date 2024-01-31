using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.AzureTable.v1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Net.Shared.Persistence.Abstractions.Models.Settings.Connections;
using Net.Shared.Persistence.Contexts;

using static KdmidScheduler.Abstractions.Constants;

namespace KdmidScheduler.Infrastructure.Persistence.AzureTable.Contexts;

public sealed class KdmidPersistenceContext(ILogger<KdmidPersistenceContext> logger, IOptions<AzureTableConnectionSettings> options) : AzureTableContext(logger, options.Value)
{
    public override void OnModelCreating(AzureTableBuilder builder)
    {
        builder.SetTable<KdmidRequestCache>();
        builder.SetTable<KdmidBotCommands>();
        builder.SetTable<KdmidAvailableDates>();
        builder.SetTable(new KdmidAvailableDatesSteps[]
        {
            new()
            {
                Id = (int)KdmidProcessSteps.CheckAvailableDates,
                Name = nameof(KdmidProcessSteps.CheckAvailableDates)
            }
        });
    }
}
