namespace Telegram.ApAzureBot.Core.Models.CommandProcesses.Kdmid;

public sealed record KdmidCommand(long ChatId, string Id, KdmidCity City, string? Parameters);
