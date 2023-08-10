using Azure.Data.Tables;

using Microsoft.Azure.Functions.Worker;

using Telegram.ApAzureBot.Worker.Models;

namespace Telegram.ApAzureBot.Worker;

public class Functions
{
    private readonly string _connectionString;
    public Functions()
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureStorageConnectionString");

        ArgumentNullException.ThrowIfNull(connectionString, "Telegram token was not found.");
        
        _connectionString = connectionString;
    }

    [Function("TelegramApAzureBotWorker")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TelegramTimer timer)
    {
        var tableClient = new TableClient(_connectionString, "TelegramApAzureBotTable");
        var result = tableClient.Query<TelegramMessageEntity>(x => x.ChatId == 123456).ToArray();
    }
}
