using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Persistence;

using Telegram.ApAzureBot.Core.Abstractions.Persistence.Repositories;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;
using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Captcha;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Html;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Http;
using Telegram.ApAzureBot.Core.Services;
using Telegram.ApAzureBot.Core.Services.CommandProcesses;
using Telegram.ApAzureBot.Infrastructure.Persistence.Context;
using Telegram.ApAzureBot.Infrastructure.Persistence.Repositories;
using Telegram.ApAzureBot.Infrastructure.Services;
using Telegram.ApAzureBot.Infrastructure.Services.Web.Captcha;
using Telegram.ApAzureBot.Infrastructure.Services.Web.Html;
using Telegram.ApAzureBot.Infrastructure.Services.Web.Http;

namespace Telegram.ApAzureBot.Infrastructure;

public static class Registrations
{
    public static void ConfigureApi(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddTelegram();
        services.AddPersistence();

#if DEBUG
        services.AddTelegramKdmidCommand();
#endif
    }
    public static void ConfigureWorker(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddTelegram();
        services.AddTelegramKdmidCommand();
        services.AddPersistence();

        services.AddTransient<ITelegramCommandTaskService, TelegramCommandTaskService>();
    }

    private static void AddPersistence(this IServiceCollection services)
    {
        services.AddAzureTable<AzureTableApAzureBotContext>(ServiceLifetime.Transient);
        services.AddTransient<ITelegramCommandTaskRepository, TelegramCommandTaskRepository>();
    }
    private static void AddTelegram(this IServiceCollection services)
    {
        services.AddSingleton<TelegramMemoryCache>();

#if DEBUG
        services.AddSingleton<ITelegramClient, TelegramExecutionClient>();
#else
        services.AddTransient<ITelegramClient, TelegramSchedulingClient>();
#endif

        services.AddTransient<ITelegramServiceProvider, TelegramServiceProvider>();
        services.AddTransient<ITelegramCommand, TelegramCommand>();
        services.AddTransient<IMenuCommandProcess, MenuCommandProcess>();
    }
    private static void AddTelegramKdmidCommand(this IServiceCollection services)
    {
        services.AddHttpClient(Core.Constants.Kdmid);
        services.AddHttpClient(Core.Constants.AntiCaptcha, x =>
        {
            x.BaseAddress = new Uri("https://api.anti-captcha.com/");
        });

        services.AddTransient<IHttpClient, KdmidHttpClient>();
        services.AddTransient<IHtmlDocument, KdmidHtmlDocument>();
        services.AddTransient<ICaptchaService, AntiCaptchaService>();
        services.AddTransient<IKdmidCommandProcess, KdmidCommandProcess>();
    }
}
