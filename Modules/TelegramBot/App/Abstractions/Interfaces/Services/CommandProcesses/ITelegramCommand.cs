namespace TelegramBot.Abstractions.Interfaces.Services.CommandProcesses;

public interface ITelegramCommand
{
    Task Start(long chatId, ReadOnlySpan<char> message, CancellationToken cToken);
}
