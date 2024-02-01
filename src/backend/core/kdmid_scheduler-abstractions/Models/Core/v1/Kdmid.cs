using System.Text.RegularExpressions;

using Net.Shared.Abstractions.Models.Exceptions;

namespace KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

public sealed record KdmidId
{
    public string Id { get; init; } = null!;
    public string Cd { get; init; } = null!;
    public string? Ems { get; init; }

    public void Validate()
    {
       if(!int.TryParse(Id, out var id) || id == 0)
           throw new UserInvalidOperationException($"Id must be a number, but was '{Id}'.");
        
        var regex = new Regex(@"^[a-zA-Z0-9]+$");
        
        if(!regex.IsMatch(Cd))
            throw new UserInvalidOperationException($"Cd must contain only letters and numbers, but was '{Cd}'.");

        if(!string.IsNullOrWhiteSpace(Ems) && !regex.IsMatch(Ems))
            throw new UserInvalidOperationException($"Ems must contain only letters and numbers, but was '{Ems}'.");
    }

    public override string ToString() => string.IsNullOrWhiteSpace(Ems)
        ? $"id={Id}&cd={Cd}"
        : $"id={Id}&cd={Cd}&ems={Ems}";
}
public sealed record City(string Code, string Name, int TimeShift);
public sealed record Attempts(int Day, byte Count);

public sealed record AvailableDatesResult(string FormData, Dictionary<string, string> Dates);
public sealed record ChosenDateResult(string FormData, string ChosenKey, string ChosenValue);

public sealed record StartPage(string FormData, string CaptchaCode);
public sealed record CalendarPage(string FormData, Dictionary<string, string> Dates);
