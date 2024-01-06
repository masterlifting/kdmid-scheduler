namespace Net.Shared.Abstractions.Models.Settings.Connection;

public abstract record ConnectionsSettings
{
    public abstract string ConnectionString { get; }
}
