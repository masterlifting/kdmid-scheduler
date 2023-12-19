using Net.Shared.Bots.Abstractions.Interfaces;

namespace Net.Shared.Bots.Telegram;

internal class TelegramBotCommandService : IBotCommandService
{
    public Task HandleAudio(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    public Task HandleCommand(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    public Task HandleContact(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    public Task HandleDocument(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    public Task HandleImage(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    public Task HandleLocation(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    public Task HandleVideo(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    public Task HandleVoice(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    public Task HandleWebApp(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
