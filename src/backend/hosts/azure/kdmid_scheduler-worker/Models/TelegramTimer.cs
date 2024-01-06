namespace KdmidScheduler.Worker.Models;

public record Schedule(bool AdjustForDST);
public record ScheduleStatus(DateTime Last, DateTime LastUpdated, DateTime Next);
public record TelegramTimer(bool IsPastDue, Schedule Schedule, ScheduleStatus ScheduleStatus);
