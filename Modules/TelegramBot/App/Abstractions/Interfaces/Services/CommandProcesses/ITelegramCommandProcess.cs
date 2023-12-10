namespace TelegramBot.Abstractions.Interfaces.Services.CommandProcesses;

public interface ITelegramCommandProcess
{
    Task Start(long chatId, ReadOnlySpan<char> message, CancellationToken cToken);
}
