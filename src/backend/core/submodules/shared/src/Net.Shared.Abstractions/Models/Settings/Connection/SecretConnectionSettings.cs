using System.ComponentModel.DataAnnotations;

namespace Net.Shared.Abstractions.Models.Settings.Connection;

public abstract record SecretConnectionSettings : ConnectionsSettings
{
    [Required] public string SecretKey { get; init; } = null!;
}
