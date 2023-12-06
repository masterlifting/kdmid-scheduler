using Net.Shared.Exceptions;

namespace Telegram.ApAzureBot.Infrastructure.Exceptions;

public sealed class ApAzureBotInfrastructureException : NetSharedException
{
    public ApAzureBotInfrastructureException(string message) : base(message)
    {
    }

    public ApAzureBotInfrastructureException(Exception exception) : base(exception)
    {
    }
}
