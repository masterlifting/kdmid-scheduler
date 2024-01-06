using KdmidScheduler.Abstractions.Interfaces.Services;
using KdmidScheduler.Infrastructure.Bots;
using KdmidScheduler.Infrastructure.Persistence.Context;
using KdmidScheduler.Infrastructure.Persistence.Repositories;
using KdmidScheduler.Infrastructure.Settings;
using KdmidScheduler.Infrastructure.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Bots;
using Net.Shared.Persistence;

namespace KdmidScheduler.Infrastructure;

public static class Registrations
{
    private static IServiceCollection AddKdmidInfrastructure(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddMemoryCache();

        services.
            AddOptions<AntiCaptchaConnectionSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration
                    .GetSection(AntiCaptchaConnectionSettings.SectionName)
                    .Bind(settings);
            });

        services.AddHttpClient(Constants.Kdmid, x =>
        {
            x.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            x.DefaultRequestHeaders.Add("Accept-Language", "en,ru;q=0.9");
            x.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/117.0");
        });

        services.AddHttpClient(Constants.AntiCaptcha, x =>
        {
            x.BaseAddress = new Uri("https://api.anti-captcha.com/");
        });

        services.AddTransient<IKdmidHttpClient, KdmidHttpClient>();
        services.AddTransient<IKdmidCaptcha, KdmidCaptchaService>();
        services.AddTransient<IKdmidHtmlDocument, KdmidHtmlDocument>();

        return services;
    }
    public static IServiceCollection AddKdmidAzureInfrastructure(this IServiceCollection services) => services
        .AddKdmidInfrastructure()
        .AddAzureTable<KdmidAzureTableContext>(ServiceLifetime.Transient)
        .AddTelegramBot(x =>
        {
            x.AddRequestHandler<KdmidBotRequestService>();
            x.AddResponseHandler<KdmidBotResponseService>();
            x.AddCommandsStore<KdmidBotCommandsAzureTableStore>();
        });
    public static IServiceCollection AddKdmidVpsInfrastructure(this IServiceCollection services) => services
        .AddKdmidInfrastructure()
        .AddMongoDb<KdmidMongoDbContext>(ServiceLifetime.Transient)
        .AddTelegramBot(x =>
            {
                x.AddRequestHandler<KdmidBotRequestService>();
                x.AddResponseHandler<KdmidBotResponseService>();
                x.AddCommandsStore<KdmidBotCommandsMongoDbStore>();
            });
}
