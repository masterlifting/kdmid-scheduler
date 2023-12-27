using Net.Shared.Exceptions;

namespace KdmidScheduler.Abstractions.Models;

public sealed class KdmidSchedulerException : NetSharedException
{
    public KdmidSchedulerException(string message) : base(message)
    {
    }

    public KdmidSchedulerException(Exception exception) : base(exception)
    {
    }
}
