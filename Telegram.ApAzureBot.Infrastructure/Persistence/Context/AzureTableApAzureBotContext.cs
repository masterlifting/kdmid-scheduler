using Microsoft.Extensions.Configuration;

using Net.Shared.Persistence.Contexts;
using Telegram.ApAzureBot.Core.Persistence.Entities;

namespace Telegram.ApAzureBot.Infrastructure.Persistence.Context;

public sealed class AzureTableApAzureBotContext : AzureTableContext
{
    public AzureTableApAzureBotContext(IConfiguration configuration) 
        : base(configuration["AzureStorageConnectionString"] ?? throw new ArgumentNullException("AzureStorageConnectionString was not found."))
    {
    }

    public override void OnModelCreating(AzureTableBuilder builder)
    {
        builder.SetTable<TelegramCommandTask>();
        builder.SetTable<TelegramCommandCache>();
    }
}
