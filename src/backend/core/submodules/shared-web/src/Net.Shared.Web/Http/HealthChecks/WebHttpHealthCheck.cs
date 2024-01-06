using System.Net.NetworkInformation;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Net.Shared.Web.Http.HealthChecks;

public sealed class WebHttpHealthCheck : IHealthCheck
{
    private readonly string _host;
    public WebHttpHealthCheck(string host) => _host = host;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Ping ping = new();
        var reply = await ping.SendPingAsync(_host).ConfigureAwait(false);

        return reply.Status != IPStatus.Success ? HealthCheckResult.Unhealthy() : HealthCheckResult.Healthy();
    }
}