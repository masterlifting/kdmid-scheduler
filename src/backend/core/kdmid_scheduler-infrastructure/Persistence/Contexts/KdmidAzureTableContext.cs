using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.AzureTable.v1;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Net.Shared.Persistence.Abstractions.Models.Settings.Connections;
using Net.Shared.Persistence.Contexts;

namespace KdmidScheduler.Infrastructure.Persistence.Contexts;

public sealed class KdmidAzureTableContext(ILogger<KdmidAzureTableContext> logger, IOptions<AzureTableConnectionSettings> options) : AzureTableContext(logger, options.Value)
{
    public override void OnModelCreating(AzureTableBuilder builder)
    {
        builder.SetTable<KdmidRequestCache>();
        builder.SetTable<KdmidBotCommand>();
    }
}
