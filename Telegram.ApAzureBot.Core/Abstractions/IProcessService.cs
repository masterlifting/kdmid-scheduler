namespace Telegram.ApAzureBot.Core.Abstractions
{
    public interface IProcessService
    {
        Task Process(long chatId, ReadOnlySpan<char> message, CancellationToken cToken);
    }
}
