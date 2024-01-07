using System.ComponentModel.DataAnnotations;

namespace KdmidScheduler.Abstractions.Models.Settings;

public sealed record AntiCaptchaConnectionSettings
{
    public const string SectionName = "AntiCaptchaConnection";
    [Required] public string ApiKey { get; init; } = null!;
}
