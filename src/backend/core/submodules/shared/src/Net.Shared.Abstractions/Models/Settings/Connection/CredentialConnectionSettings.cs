using System.ComponentModel.DataAnnotations;

namespace Net.Shared.Abstractions.Models.Settings.Connection;

public abstract record CredentialConnectionSettings : ConnectionsSettings
{
    [Required] public string User { get; init; } = null!;
    [Required] public string Password { get; init; } = null!;
}
