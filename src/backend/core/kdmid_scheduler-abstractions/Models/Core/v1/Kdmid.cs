﻿namespace KdmidScheduler.Abstractions.Models.Core.v1;

public sealed record KdmidId
{
    public string Id { get; init; } = null!;
    public string Cd { get; init; } = null!;
    public string? Ems { get; init; }

    public override string ToString() => string.IsNullOrWhiteSpace(Ems)
        ? $"id={Id}&cd={Cd}"
        : $"id={Id}&cd={Cd}&ems={Ems}";
}
public sealed record City(string Code, string Name);

public sealed record AvailableDatesResult(string FormData, Dictionary<DateTime, string> Dates);
public sealed record ChosenDateResult(string FormData, DateTime Date, string ChosenValue);
public sealed record ConfirmationResult(bool IsSuccess, string Message);

public sealed record StartPage(string FormData, string CaptchaCode);
public sealed record CalendarPage(string FormData, IDictionary<string, string> Dates);
public sealed record ConfirmationPage(string Result);
