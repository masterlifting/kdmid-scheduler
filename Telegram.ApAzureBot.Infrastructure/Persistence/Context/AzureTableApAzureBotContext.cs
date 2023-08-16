using Microsoft.Extensions.Configuration;

using Net.Shared.Persistence.Contexts;

using Telegram.ApAzureBot.Core.Persistence.NoSql;

namespace Telegram.ApAzureBot.Infrastructure.Persistence.Context;

public sealed class AzureTableApAzureBotContext : AzureTableContext
{
    public AzureTableApAzureBotContext(IConfiguration configuration) 
        : base(configuration["AzureStorageConnectionString"] ?? throw new ArgumentNullException("AzureStorageConnectionString was not found."))
    {
    }

    override public void OnModelCreating(AzureTableBuilder builder)
    {
        builder.SetTable<TelegramCommandTask>();
    }
}
