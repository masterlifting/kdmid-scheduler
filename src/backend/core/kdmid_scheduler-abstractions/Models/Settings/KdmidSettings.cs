using System.ComponentModel.DataAnnotations;

namespace KdmidScheduler.Abstractions.Models.Settings;

public sealed record KdmidSettings
{
    public const string SectionName = "Kdmid";
    [Required] public string WebAppUrl { get; init; } = null!;
}
