namespace Telegram.ApAzureBot.Core;

public static class Constants
{
    public const string Kdmid = "kdmid";

    public enum TelegramCommandTaskStatus
    {
        Created = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4
    }
    public  enum TelegramCommandTaskStep
    {
       Process = 1
    }
}
