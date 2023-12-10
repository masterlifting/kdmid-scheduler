using Net.Shared.Exceptions;

namespace TelegramBot.Abstractions.Models.Exceptions;

public sealed class TelegramBotException : NetSharedException
{
    public TelegramBotException(string message) : base(message)
    {
    }

    public TelegramBotException(Exception exception) : base(exception)
    {
    }
}
