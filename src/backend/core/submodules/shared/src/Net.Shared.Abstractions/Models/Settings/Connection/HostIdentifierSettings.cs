using System;

namespace Net.Shared.Abstractions.Models.Settings.Connection
{
    public sealed record HostIdentifierSettings
    {
        public const string Name = "Host";
        public Guid Id { get; init; }
    }
}
