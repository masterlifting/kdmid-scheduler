
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

namespace KdmidScheduler.Abstractions;

public static class Constants
{
    public static readonly string BotCommandParametersCityKey = typeof(City).FullName![(typeof(City).FullName!.IndexOf(".v1") + 1)..];
    public static readonly string BotCommandParametersKdmidIdKey = typeof(KdmidId).FullName![(typeof(KdmidId).FullName!.IndexOf(".v1") + 1)..];
    public static readonly string BotCommandParametersChosenResultKey = typeof(ChosenDateResult).FullName![(typeof(ChosenDateResult).FullName!.IndexOf(".v1") + 1)..];
    public static readonly string BotCommandParametersAttemptsKey = typeof(Attempts).FullName![(typeof(Attempts).FullName!.IndexOf(".v1") + 1)..];
    public const byte KdmidRequestAttemptsLimit = 23;

    public static class KdmidBotCommandNames
    {
        public const string Mine = "mine";
        public const string CommandInProcess = "commandInProcess";
        public const string CreateCommand = "addCommand";
        public const string UpdateCommand = "updateCommand";
        public const string DeleteCommand = "deleteCommand";
        public const string SendAvailableDates = "sendAvailableDates";
        public const string SendConfirmationResult = "sendConfirmResult";
    }
    public enum KdmidProcessSteps
    {
        CheckAvailableDates = 1
    }
}
