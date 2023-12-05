namespace Telegram.ApAzureBot.Worker.Models;

public record TelegramTimer(bool IsPastDue, Schedule Schedule, ScheduleStatus ScheduleStatus);
public record Schedule(bool AdjustForDST);
public record ScheduleStatus(DateTime Last, DateTime LastUpdated, DateTime Next);
