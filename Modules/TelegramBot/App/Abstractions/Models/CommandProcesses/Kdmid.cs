namespace TelegramBot.Abstractions.Models.CommandProcesses.Kdmid;

public sealed record City(string Id, string Code, string Name);
public sealed record Command(long ChatId, string Id, City City, string? Parameters);

public sealed record Start(string FormData, string CaptchaCode);
public sealed record Calendar(string FormData, IDictionary<string, string> Variants);
public sealed record Confirmation(string Result);
