namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotCommandService
{
    Task HandleCommand(IBotMessage message, CancellationToken cToken);
    Task HandleImage(IBotMessage message, CancellationToken cToken);
    Task HandleAudio(IBotMessage message, CancellationToken cToken);
    Task HandleVideo(IBotMessage message, CancellationToken cToken);
    Task HandleDocument(IBotMessage message, CancellationToken cToken);
    Task HandleVoice(IBotMessage message, CancellationToken cToken);
    Task HandleLocation(IBotMessage message, CancellationToken cToken);
    Task HandleContact(IBotMessage message, CancellationToken cToken);
    Task HandleWebApp(IBotMessage message, CancellationToken cToken);
}
