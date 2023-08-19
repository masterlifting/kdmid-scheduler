namespace Telegram.ApAzureBot.Core.Models.CommandProcesses;

public sealed record KdmidCommand(long ChatId, string City, string? Parameters);
