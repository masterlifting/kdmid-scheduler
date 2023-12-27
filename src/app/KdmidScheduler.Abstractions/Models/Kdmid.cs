namespace KdmidScheduler.Abstractions.Models;

public sealed record Identifier
{
    public string Id { get; }
    public string Cd { get; }
    public string? Ems { get; }

    public Identifier()
    {
        Id = string.Empty;
        Cd = string.Empty;
        Ems = null;
    }

    public Identifier(string input)
    {
        var isValidIdentifier =
                input.StartsWith("id=")
                && input.IndexOf('&') > 0
                && input.IndexOf("cd=") > 0;

        if (!isValidIdentifier)
            throw new ArgumentException("The input string is not a valid KdmidId identifier.", nameof(input));

        Id = input[3..input.IndexOf('&')];
        Cd = input[(input.IndexOf("cd=") + 3)..];
        Ems = input.IndexOf("ems=") > 0 ? input[(input.IndexOf("ems=") + 4)..] : null;
    }

    public override string ToString() => string.IsNullOrWhiteSpace(Ems)
        ? $"id={Id}&cd={Cd}"
        : $"id={Id}&cd={Cd}&ems={Ems}";
}
public sealed record City(string Code, string Name);

public sealed record AvailableDates(string FormData, Dictionary<DateTime, string> Dates);
public sealed record ChosenDateResult(string FormData, DateTime Date, string ChosenValue);
public sealed record ConfirmationResult(bool IsSuccess, string Message);

public sealed record StartPage(string FormData, string CaptchaCode);
public sealed record CalendarPage(string FormData, IDictionary<string, string> Variants);
public sealed record ConfirmationPage(string Result);
