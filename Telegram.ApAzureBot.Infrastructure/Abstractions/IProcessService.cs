namespace Telegram.ApAzureBot.Infrastructure.Abstractions
{
    public interface IProcessService
    {
        Task Process(long chatId, ReadOnlySpan<char> message, CancellationToken cToken);
    }
}
