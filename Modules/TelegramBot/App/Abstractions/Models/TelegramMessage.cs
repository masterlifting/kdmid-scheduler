namespace TelegramBot.Abstractions.Models;

public sealed class TelegramMessage
{
    public TelegramMessage() { }

    public TelegramMessage(long chatId, string text)
    {
        ChatId = chatId;
        Text = text;
    }

    public long ChatId { get; init; }
    public string Text { get; init; } = null!;
}
