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
    public static IServiceCollection AddKdmidInfrastructure(this IServiceCollection services)
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

        services.AddHttpClient(Constants.Kdmid);

        services.AddHttpClient(Constants.AntiCaptcha, x =>
        {
            x.BaseAddress = new Uri("https://api.anti-captcha.com/");
        });

        services.AddTransient< IPersistenceWriterRepository<KdmidAvailableDates>, MongoDbWriterRepository<KdmidMongoDbContext, KdmidAvailableDates>>();

        services.AddTransient<IKdmidHttpClient, KdmidHttpClient>();
        services.AddTransient<IKdmidCaptcha, KdmidCaptchaService>();
        services.AddTransient<IKdmidHtmlDocument, KdmidHtmlDocument>();

        return services;
    }
    public static IServiceCollection AddKdmidAzureInfrastructure(this IServiceCollection services) => services
        .AddAzureTable<KdmidAzureTableContext>(ServiceLifetime.Transient)
        .AddTelegramBot<KdmidBotResponse>(x =>
        {
            x.Services.AddTransient<IPersistenceReaderRepository<KdmidBotCommand>, AzureTableReaderRepository<KdmidAzureTableContext, KdmidBotCommand>>();
            x.Services.AddTransient<IPersistenceWriterRepository<KdmidBotCommand>, AzureTableWriterRepository<KdmidAzureTableContext, KdmidBotCommand>>();
            x.AddCommandsStore<Bots.Stores.AzureTable.KdmidBotCommandsStore>();
        });
    public static IServiceCollection AddKdmidVpsInfrastructure(this IServiceCollection services) => services
        .AddMongoDb<KdmidMongoDbContext>(ServiceLifetime.Scoped)
        .AddTelegramBot<KdmidBotResponse>(x =>
        {
            x.Services.AddTransient<IPersistenceReaderRepository<KdmidBotCommands>, MongoDbReaderRepository<KdmidMongoDbContext, KdmidBotCommands>>();
            x.Services.AddTransient<IPersistenceWriterRepository<KdmidBotCommands>, MongoDbWriterRepository<KdmidMongoDbContext, KdmidBotCommands>>();
            x.AddCommandsStore<Bots.Stores.MongoDb.KdmidBotCommandsStore>();
        });
}
