using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Services;
using KdmidScheduler.Abstractions.Models.Settings;
using KdmidScheduler.Infrastructure.Bots;
using KdmidScheduler.Infrastructure.Persistence.Contexts;
using KdmidScheduler.Infrastructure.Web;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Net.Shared.Bots;
using Net.Shared.Persistence;

namespace KdmidScheduler.Infrastructure;

public static class Registrations
{
    public static IServiceCollection AddKdmidInfrastructure(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddMemoryCache();

        services
            .AddOptions<AntiCaptchaConnectionSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration
                    .GetSection(AntiCaptchaConnectionSettings.SectionName)
                    .Bind(settings);
            })
            .ValidateOnStart()
            .Validate(x => !string.IsNullOrWhiteSpace(x.ApiKey), "Api key of AntiCaptcha should not be empty.");

        services
            .AddOptions<KdmidSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration
                    .GetSection(KdmidSettings.SectionName)
                    .Bind(settings);

            })
            .ValidateOnStart()
            .Validate(x => !string.IsNullOrWhiteSpace(x.WebAppUrl), "Web app url of Kdmid should not be empty.");

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
        .AddAzureTable<KdmidAzureTableContext>(ServiceLifetime.Transient)
        .AddTelegramBot<KdmidBotResponse>(x =>
        {
            x.AddCommandsStore<Bots.Stores.AzureTable.KdmidBotCommandsStore>();
        });
    public static IServiceCollection AddKdmidVpsInfrastructure(this IServiceCollection services) => services
        .AddMongoDb<KdmidMongoDbContext>(ServiceLifetime.Transient)
        .AddTelegramBot<KdmidBotResponse>(x =>
        {
            //x.AddCommandsStore<Bots.Stores.MongoDb.KdmidBotCommandsStore>();
        })
        .AddCors(options =>
        {
            var kdmidSettings = services
                .BuildServiceProvider()
                .GetRequiredService<IOptions<KdmidSettings>>().Value;

            options.AddPolicy(Constants.TelegramWebAppCorsPolicy, builder =>
            {
                builder
                    .WithOrigins(kdmidSettings.WebAppUrl)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
}
