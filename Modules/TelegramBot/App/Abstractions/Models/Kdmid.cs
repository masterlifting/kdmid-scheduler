namespace TelegramBot.Abstractions.Models;

public sealed record City(string Id, string Code, string Name);
public sealed record Embassy(City City, string Name);

public sealed record StartPageResult(string FormData, string CaptchaCode);
public sealed record CalendarPageResult(string FormData, IDictionary<string, string> Variants);
public sealed record ConfirmationPageResult(string Result);
