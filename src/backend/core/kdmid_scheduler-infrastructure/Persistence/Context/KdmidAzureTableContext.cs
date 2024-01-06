using KdmidScheduler.Abstractions.Models.v1.Persistence.AzureTable;

using Microsoft.Extensions.Options;

using Net.Shared.Persistence.Abstractions.Models.Settings.Connections;
using Net.Shared.Persistence.Contexts;

namespace KdmidScheduler.Infrastructure.Persistence.Context;

public sealed class KdmidAzureTableContext(IOptions<AzureTableConnectionSettings> options) : AzureTableContext(options.Value)
{
    public override void OnModelCreating(AzureTableBuilder builder)
    {
        builder.SetTable<KdmidBotCommand>();
    }
}
