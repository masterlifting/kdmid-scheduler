using Net.Shared.Exceptions;

namespace Telegram.ApAzureBot.Core.Exceptions;

public sealed class ApAzureBotCoreException : NetSharedException
{
    public ApAzureBotCoreException(string message) : base(message)
    {
    }

    public ApAzureBotCoreException(Exception exception) : base(exception)
    {
    }
}
