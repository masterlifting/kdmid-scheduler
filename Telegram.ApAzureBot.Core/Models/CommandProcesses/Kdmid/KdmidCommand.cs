namespace Telegram.ApAzureBot.Core.Models.CommandProcesses.Kdmid;

public sealed record KdmidCommand(long ChatId, KdmidCity City, string? Parameters);
