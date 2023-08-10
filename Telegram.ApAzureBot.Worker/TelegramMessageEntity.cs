using Azure;
using Azure.Data.Tables;

namespace Telegram.ApAzureBot.Worker
{
    public sealed class TelegramMessageEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "apazurebot";
        public string RowKey { get; set; } = Guid.NewGuid().ToString("N");
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public long ChatId { get; set; }
        public string Text { get; set; }
    }
}
