
namespace KdmidScheduler.Abstractions;

public static class Constants
{
    public static class KdmidBotCommands
    {
        public const string Mine = "mine";
        public const string CommandInProcess = "commandInProcess";
        public const string CreateCommand = "addCommand";
        public const string UpdateCommand = "updateCommand";
        public const string DeleteCommand = "deleteCommand";
        public const string SendAvailableDates = "sendAvailableDates";
        public const string SendConfirmResult = "sendConfirmResult";
    }
    public enum KdmidProcessSteps
    {
        CheckAvailableDates = 1
    }
}
