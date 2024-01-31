using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Web;
using KdmidScheduler.Abstractions.Models.Settings;
using KdmidScheduler.Infrastructure.Bots;
using KdmidScheduler.Infrastructure.Web;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Net.Shared;
using Net.Shared.Bots;
using Net.Shared.Persistence;
using Net.Shared.Persistence.Repositories.AzureTable;
using Net.Shared.Persistence.Repositories.MongoDb;

using PersistenceInfrastructure = KdmidScheduler.Infrastructure.Persistence;
using PersistenceModels = KdmidScheduler.Abstractions.Models.Infrastructure.Persistence;

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
            .AddScoped<IKdmidRequestCaptcha, KdmidRequestCaptchaService>()
            .AddScoped<IKdmidRequestHttpClient, KdmidRequestHttpClient>()
            .AddScoped<IKdmidRequestHtmlDocument, KdmidRequestHtmlDocument>();
    }

    public static IServiceCollection AddKdmidAzureInfrastructure(this IServiceCollection services) => services
        .AddKdmidInfrastructure()
        .AddAzureTable<PersistenceInfrastructure.AzureTable.Contexts.KdmidPersistenceContext>(ServiceLifetime.Scoped)
        .AddScoped<AzureTableReaderRepository<PersistenceInfrastructure.AzureTable.Contexts.KdmidPersistenceContext, PersistenceModels.AzureTable.v1.KdmidAvailableDates>>()
        .AddScoped<AzureTableReaderRepository<PersistenceInfrastructure.AzureTable.Contexts.KdmidPersistenceContext, PersistenceModels.AzureTable.v1.KdmidRequestCache>>()
        .AddScoped<AzureTableWriterRepository<PersistenceInfrastructure.AzureTable.Contexts.KdmidPersistenceContext, PersistenceModels.AzureTable.v1.KdmidRequestCache>>()
        .AddScoped<AzureTableReaderRepository<PersistenceInfrastructure.AzureTable.Contexts.KdmidPersistenceContext, PersistenceModels.AzureTable.v1.KdmidBotCommands>>()
        .AddScoped<AzureTableWriterRepository<PersistenceInfrastructure.AzureTable.Contexts.KdmidPersistenceContext, PersistenceModels.AzureTable.v1.KdmidBotCommands>>()
        .AddTelegramBot<KdmidBotResponse>(x =>
        {
            x.AddCommandsStore<PersistenceInfrastructure.AzureTable.Repositories.Bots.KdmidBotCommandsStore>();
        });
    public static IServiceCollection AddKdmidVpsInfrastructure(this IServiceCollection services) => services
        .AddKdmidInfrastructure()
        .AddMongoDb<PersistenceInfrastructure.MongoDb.Contexts.KdmidPersistenceContext>(ServiceLifetime.Scoped)
        .AddScoped<MongoDbWriterRepository<PersistenceInfrastructure.MongoDb.Contexts.KdmidPersistenceContext, PersistenceModels.MongoDb.v1.KdmidAvailableDates>>()
        .AddScoped<MongoDbReaderRepository<PersistenceInfrastructure.MongoDb.Contexts.KdmidPersistenceContext, PersistenceModels.MongoDb.v1.KdmidRequestCache>>()
        .AddScoped<MongoDbWriterRepository<PersistenceInfrastructure.MongoDb.Contexts.KdmidPersistenceContext, PersistenceModels.MongoDb.v1.KdmidRequestCache>>()
        .AddScoped<MongoDbReaderRepository<PersistenceInfrastructure.MongoDb.Contexts.KdmidPersistenceContext, PersistenceModels.MongoDb.v1.KdmidBotCommands>>()
        .AddScoped<MongoDbWriterRepository<PersistenceInfrastructure.MongoDb.Contexts.KdmidPersistenceContext, PersistenceModels.MongoDb.v1.KdmidBotCommands>>()
        .AddTelegramBot<KdmidBotResponse>(x =>
        {
            x.AddCommandsStore<PersistenceInfrastructure.MongoDb.Repositories.Bots.KdmidBotCommandsStore>();
        });
}
