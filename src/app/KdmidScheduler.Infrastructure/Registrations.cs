using KdmidScheduler.Abstractions.Interfaces;
using KdmidScheduler.Infrastructure.Services;

using Microsoft.Extensions.DependencyInjection;

namespace KdmidScheduler.Infrastructure;

public static class Registrations
{
    public static IServiceCollection AddKdmidInfrastructure(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddMemoryCache();

        //services.AddAzureTable<AzureTableApAzureBotContext>(ServiceLifetime.Transient);
        //services.AddTransient<ITelegramCommandTaskRepository, TelegramCommandTaskRepository>();

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
}
