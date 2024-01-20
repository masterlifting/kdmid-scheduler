using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

namespace KdmidScheduler;

public static class Constants
{
    public static readonly string BotCommandParametersCityKey = typeof(City).FullName![(typeof(City).FullName!.IndexOf(".v1") + 1)..];
    public static readonly string BotCommandParametersKdmidIdKey = typeof(KdmidId).FullName![typeof(KdmidId).FullName!.IndexOf(".v1")..];
    public static readonly string BotCommandParametersChosenResultKey = typeof(ChosenDateResult).FullName![typeof(ChosenDateResult).FullName!.IndexOf(".v1")..];
    public static readonly string BotCommandParametersAttemptsKey = typeof(Attempts).FullName![typeof(Attempts).FullName!.IndexOf(".v1")..];
    public const byte AttemptsLimit = 23;
}
