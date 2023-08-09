namespace Telegram.ApAzureBot.Worker.Models;

public class TelegramScheduleStatus
{
    public DateTime Last { get; set; }

    public DateTime Next { get; set; }

    public DateTime LastUpdated { get; set; }
}
