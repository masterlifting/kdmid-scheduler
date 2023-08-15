using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Persistence;

using Telegram.ApAzureBot.Core.Abstractions.Services.CommandServices;
using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Captcha;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Html;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Http;
using Telegram.ApAzureBot.Core.Persistence;
using Telegram.ApAzureBot.Core.Services.CommandServices;
using Telegram.ApAzureBot.Core.Services.Telegram;
using Telegram.ApAzureBot.Infrastructure.Persistence.Context;
using Telegram.ApAzureBot.Infrastructure.Persistence.Repository;
using Telegram.ApAzureBot.Infrastructure.Services.Telegram;
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
    }
    public static void ConfigureWorker(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddTelegram();
        services.AddTelegramKdmidCommand();
        services.AddPersistence();
    }

    private static void AddPersistence(this IServiceCollection services)
    {
        services.AddAzureTable<AzureTableApAzureBotContext>(ServiceLifetime.Transient);
        services.AddTransient<ITelegramCommandTaskRepository, TelegramCommandTaskRepository>();
    }
    private static void AddTelegram(this IServiceCollection services)
    {
        services.AddSingleton<TelegramMemoryCache>();
        services.AddTransient<ITelegramClient, TelegramSchedulingClient>();

        services.AddTransient<ITelegramServiceProvider, TelegramServiceProvider>();
        services.AddTransient<ITelegramCommand, TelegramCommand>();
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
