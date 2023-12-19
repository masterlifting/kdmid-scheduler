using System.Text;

using Net.Shared.Bots.Abstractions.Interfaces;

using static Net.Shared.Bots.Abstractions.Constants;

namespace Net.Shared.Bots.Telegram;

public sealed class TelegramBotCommand(IBotCommandService service) : IBotCommand
{
    private readonly IBotCommandService _service = service;

    public Task Process(IBotMessage message, CancellationToken cToken) => message.Type switch
    {
        BotMessageType.Audio => _service.HandleAudio(message, cToken),
        BotMessageType.Command => _service.HandleCommand(message, cToken),
        BotMessageType.Contact => _service.HandleContact(message, cToken),
        BotMessageType.Document => _service.HandleDocument(message, cToken),
        BotMessageType.Image => _service.HandleImage(message, cToken),
        BotMessageType.Location => _service.HandleLocation(message, cToken),
        BotMessageType.Video => _service.HandleVideo(message, cToken),
        BotMessageType.Voice => _service.HandleVoice(message, cToken),
        BotMessageType.WebApp => _service.HandleWebApp(message, cToken),
        _ => throw new NotSupportedException($"Message type {message.Type} is not supported.")
    };
}
