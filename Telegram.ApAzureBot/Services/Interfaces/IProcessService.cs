namespace Telegram.ApAzureBot.Services.Interfaces;

public interface IProcessService
{
    Task Process(long chatId, ReadOnlySpan<char> message, CancellationToken cToken);
}
