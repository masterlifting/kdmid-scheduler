using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Services;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.AzureTable.v1;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;
using KdmidScheduler.Abstractions.Models.Settings;
using KdmidScheduler.Infrastructure.Bots;
using KdmidScheduler.Infrastructure.Persistence.Contexts;
using KdmidScheduler.Infrastructure.Web;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Net.Shared;
using Net.Shared.Bots;
using Net.Shared.Persistence;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;
using Net.Shared.Persistence.Repositories.AzureTable;
using Net.Shared.Persistence.Repositories.MongoDb;

namespace KdmidScheduler.Infrastructure;

public static class Registrations
{
    private static IServiceCollection AddKdmidInfrastructure(this IServiceCollection services)
    {
        services
            .AddLogging()
            .AddMemoryCache()
            .AddCorrelationSettings();

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
            x.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            x.DefaultRequestHeaders.Add("Accept-Language", "en,ru;q=0.9");
            x.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            x.DefaultRequestHeaders.Add("Sec-Ch-Ua", "Not A(Brand\";v=\"99\", \"Microsoft Edge\";v=\"121\", \"Chromium\";v=\"121");
            x.DefaultRequestHeaders.Add("Sec-Ch-Ua-Mobile", "?0");
            x.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
            x.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            x.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            x.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
            x.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            x.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            x.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");
        });

        services.AddHttpClient(Constants.AntiCaptcha, x =>
        {
            x.BaseAddress = new Uri("https://api.anti-captcha.com/");
        });

        return services
            .AddScoped<IKdmidHttpClient, KdmidHttpClient>()
            .AddScoped<IKdmidCaptcha, KdmidCaptchaService>()
            .AddScoped<IKdmidHtmlDocument, KdmidHtmlDocument>();
    }

    public static IServiceCollection AddKdmidAzureInfrastructure(this IServiceCollection services) => services
        .AddKdmidInfrastructure()
        .AddAzureTable<KdmidAzureTableContext>(ServiceLifetime.Scoped)
        .AddScoped<
            IPersistenceReaderRepository<Abstractions.Models.Infrastructure.Persistence.AzureTable.v1.KdmidRequestCache>, 
            AzureTableReaderRepository<KdmidAzureTableContext, Abstractions.Models.Infrastructure.Persistence.AzureTable.v1.KdmidRequestCache>>()
        .AddScoped<
            IPersistenceWriterRepository<Abstractions.Models.Infrastructure.Persistence.AzureTable.v1.KdmidRequestCache>, 
            AzureTableWriterRepository<KdmidAzureTableContext, Abstractions.Models.Infrastructure.Persistence.AzureTable.v1.KdmidRequestCache>>()
        .AddTelegramBot<KdmidBotResponse>(x =>
        {
            x.Services.AddScoped<IPersistenceReaderRepository<KdmidBotCommand>, AzureTableReaderRepository<KdmidAzureTableContext, KdmidBotCommand>>();
            x.Services.AddScoped<IPersistenceWriterRepository<KdmidBotCommand>, AzureTableWriterRepository<KdmidAzureTableContext, KdmidBotCommand>>();
            x.AddCommandsStore<Bots.Stores.AzureTable.KdmidBotCommandsStore>();
        });
    public static IServiceCollection AddKdmidVpsInfrastructure(this IServiceCollection services) => services
        .AddKdmidInfrastructure()
        .AddMongoDb<KdmidMongoDbContext>(ServiceLifetime.Scoped)
        .AddScoped<
            IPersistenceReaderRepository<Abstractions.Models.Infrastructure.Persistence.MongoDb.v1.KdmidRequestCache>, 
            MongoDbReaderRepository<KdmidMongoDbContext, Abstractions.Models.Infrastructure.Persistence.MongoDb.v1.KdmidRequestCache>>()
        .AddScoped<
            IPersistenceWriterRepository<Abstractions.Models.Infrastructure.Persistence.MongoDb.v1.KdmidRequestCache>, 
            MongoDbWriterRepository<KdmidMongoDbContext, Abstractions.Models.Infrastructure.Persistence.MongoDb.v1.KdmidRequestCache>>()
        .AddScoped<
            IPersistenceWriterRepository<KdmidAvailableDates>, 
            MongoDbWriterRepository<KdmidMongoDbContext, KdmidAvailableDates>>()
        .AddTelegramBot<KdmidBotResponse>(x =>
        {
            x.Services.AddScoped<IPersistenceReaderRepository<KdmidBotCommands>, MongoDbReaderRepository<KdmidMongoDbContext, KdmidBotCommands>>();
            x.Services.AddScoped<IPersistenceWriterRepository<KdmidBotCommands>, MongoDbWriterRepository<KdmidMongoDbContext, KdmidBotCommands>>();
            x.AddCommandsStore<Bots.Stores.MongoDb.KdmidBotCommandsStore>();
        });
}
