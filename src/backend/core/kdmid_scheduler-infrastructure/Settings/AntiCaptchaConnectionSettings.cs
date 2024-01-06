using System.ComponentModel.DataAnnotations;

namespace KdmidScheduler.Infrastructure.Settings;

public sealed record AntiCaptchaConnectionSettings
{
    public const string SectionName = "AntiCaptchaConnection";
    [Required] public string ApiKey { get; init; } = null!;
}
