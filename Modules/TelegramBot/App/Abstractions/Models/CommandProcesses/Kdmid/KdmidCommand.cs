namespace TelegramBot.Abstractions.Models.CommandProcesses.Kdmid;

public sealed record KdmidCommand(long ChatId, string Id, KdmidCity City, string? Parameters);
