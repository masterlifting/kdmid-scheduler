namespace Telegram.ApAzureBot.Core.Models;

public sealed class TelegramPhoto
{
    public TelegramPhoto() { }

    public TelegramPhoto(long chatId, byte[] payload, string name, string? description = null)
    {
        ChatId = chatId;
        Payload = payload;
        Name = name;
        Description = description;
    }

    public long ChatId { get; init; }
    public byte[] Payload { get; init; } = Array.Empty<byte>();
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
