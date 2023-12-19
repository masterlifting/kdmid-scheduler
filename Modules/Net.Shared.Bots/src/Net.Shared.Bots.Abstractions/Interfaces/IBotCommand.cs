namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotCommand
{
    Task Process(IBotMessage message, CancellationToken cToken);
}
