using KdmidScheduler.Abstractions.Models.Settings;

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace KdmidScheduler.Infrastructure.Options;

public sealed class ConfigureCorsOptions(IOptions<KdmidSettings> options) : IConfigureOptions<CorsOptions>
{
    public const string PolicyName = "TelegramWebAppCorsPolicy";

    private readonly KdmidSettings _kdmidSettings = options.Value;

    public void Configure(CorsOptions options)
    {
        options.AddPolicy(PolicyName, builder =>
        {
            builder
                .WithOrigins(_kdmidSettings.WebAppUrl)
                .WithMethods("GET", "POST")
                .AllowCredentials()
                .WithHeaders("Content-Type");
        });
    }
}
