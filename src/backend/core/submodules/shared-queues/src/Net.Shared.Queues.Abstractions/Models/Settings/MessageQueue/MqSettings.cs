using Net.Shared.Background.Abstractions.Models.Settings;

namespace Net.Shared.Queues.Abstractions.Models.Settings.MessageQueue;

public abstract record MqSettings
{
    public string Queue { get; init; } = null!;
    public uint Limit { get; init; } = 1_000;
    public BackgroundTaskSettings TaskSettings { get; init; } = new();
}
