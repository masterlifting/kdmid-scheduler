using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Persistence;

using Telegram.ApAzureBot.Core.Abstractions.Persistence.Repositories;
using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;
using Telegram.ApAzureBot.Core.Services;
using Telegram.ApAzureBot.Core.Services.CommandProcesses;
using Telegram.ApAzureBot.Infrastructure.Persistence.Context;
using Telegram.ApAzureBot.Infrastructure.Persistence.Repositories;
using Telegram.ApAzureBot.Infrastructure.Services;
using Telegram.ApAzureBot.Infrastructure.Services.CommandProcesses.Kdmid;

namespace Telegram.ApAzureBot.Infrastructure;

public static class Registrations
{
    public static void ConfigureApi(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddTelegram();
        services.AddPersistence();

        #if DEBUG
                services.AddTelegramKdmid();
        #endif
    }
    public static void ConfigureWorker(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddTelegram();
        services.AddTelegramKdmid();
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
        services.AddTransient<ITelegramServiceProvider, TelegramServiceProvider>();

        services.AddTransient<ITelegramCommand, TelegramCommand>();
        services.AddTransient<ITelegramMenuCommandProcess, TelegramMenuCommandProcess>();

        #if DEBUG
                services.AddSingleton<ITelegramClient, TelegramClient>();
        #else
                services.AddTransient<ITelegramClient, TelegramClient>();
        #endif
    }
    private static void AddTelegramKdmid(this IServiceCollection services)
    {
        services.AddHttpClient(Core.Constants.Kdmid);
        services.AddHttpClient(Core.Constants.AntiCaptcha, x =>
        {
            x.BaseAddress = new Uri("https://api.anti-captcha.com/");
        });

        services.AddTransient<IKdmidHttpClient, KdmidHttpClient>();
        services.AddTransient<IKdmidHtmlDocument, KdmidHtmlDocument>();
        services.AddTransient<IKdmidCaptchaService, KdmidCaptchaService>();
        services.AddTransient<IKdmidCommandProcess, KdmidCommandProcess>();
    }
}
