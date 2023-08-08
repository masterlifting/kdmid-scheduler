namespace Telegram.ApAzureBot.Core.Abstractions
{
    public interface IClient
    {
        ITelegramClient Client { get; }
        Task SetWebhook(string url, CancellationToken cToken);
        Task ListenMessages(CancellationToken cToken);
        Task ReceiveMessage(string data, CancellationToken cToken);
        Task SendMessage(long chatId, string message, CancellationToken cToken);
        Task SendPhoto(long chatId, byte[] payload, string description , CancellationToken cToken);
    }
}
