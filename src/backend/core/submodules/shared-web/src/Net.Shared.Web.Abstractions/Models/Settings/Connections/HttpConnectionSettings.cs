namespace Net.Shared.Web.Abstractions.Models.Settings.Connections;

public sealed class HttpConnectionSettings
{
    public string Schema { get; set; } = null!;
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string? ApiKey { get; set; }
}