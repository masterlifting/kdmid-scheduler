using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Persistence.Abstractions.Repositories.NoSql;

using Telegram.ApAzureBot.Core.Abstractions.Services.CommandServices;
using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Captcha;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Html;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Http;
using Telegram.ApAzureBot.Core.Services.CommandServices;
using Telegram.ApAzureBot.Core.Services.Telegram;
using Telegram.ApAzureBot.Infrastructure.Persistence.Repository;
using Telegram.ApAzureBot.Infrastructure.Services.Telegram;
using Telegram.ApAzureBot.Infrastructure.Services.Web.Captcha;
using Telegram.ApAzureBot.Infrastructure.Services.Web.Html;
using Telegram.ApAzureBot.Infrastructure.Services.Web.Http;

namespace Telegram.ApAzureBot.Infrastructure;

public static class Registrations
{
    public static IServiceCollection AddApAzureBotApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging();
        services.AddHttpClient(Core.Constants.Kdmid);
        services.AddSingleton<TelegramMemoryCache>();

#if DEBUG
        services.AddSingleton<ITelegramClient, TelegramClient>();
#else
        services.AddScoped<ITelegramClient, TelegramClient>();
#endif

        services.AddTransient<ITelegramServiceProvider, TelegramServiceProvider>();
        services.AddTransient<IHtmlDocument, KdmidHtmlDocument>();
        services.AddTransient<IHttpClient, KdmidHttpClient>();
        services.AddTransient<ITelegramCommand, TelegramCommand>();
        services.AddTransient<ICaptchaService, AntiCaptchaService>();
        services.AddTransient<IKdmidService, KdmidTelegramCommand>();

        return services;
    }
    public static IServiceCollection AddApAzureBotWorkerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ITelegramCommand, TelegramCommand>();

        services.AddPersistenceServices(configuration);

        return services;
    }

    private static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IPersistenceNoSqlProcessRepository, PersistenceNoSqlProcessRepository>();

        return services;
    }
}
