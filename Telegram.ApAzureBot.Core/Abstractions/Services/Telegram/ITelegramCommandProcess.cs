namespace Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;

public interface ITelegramCommandProcess
{
    Task Start(long chatId, ReadOnlySpan<char> message, CancellationToken cToken);
}
