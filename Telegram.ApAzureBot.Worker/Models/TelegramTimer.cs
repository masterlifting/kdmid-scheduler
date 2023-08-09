namespace Telegram.ApAzureBot.Worker.Models;

public class TelegramTimer
{
    public TelegramScheduleStatus? Status { get; set; }

    public bool IsPastDue { get; set; }
}
