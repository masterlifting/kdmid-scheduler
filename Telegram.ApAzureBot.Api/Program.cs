using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Telegram.ApAzureBot.Core;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandServices;
using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Captcha;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Html;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Http;
using Telegram.ApAzureBot.Core.Services.CommandServices;
using Telegram.ApAzureBot.Core.Services.Telegram;
using Telegram.ApAzureBot.Infrastructure.Services.Telegram;
using Telegram.ApAzureBot.Infrastructure.Services.Web.Captcha;
using Telegram.ApAzureBot.Infrastructure.Services.Web.Html;
using Telegram.ApAzureBot.Infrastructure.Services.Web.Http;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((context, builder) =>
    {
        builder.Services.AddLogging();
        builder.Services.AddHttpClient(Constants.Kdmid);
        builder.Services.AddSingleton<TelegramMemoryCache>();

        #if DEBUG
            builder.Services.AddSingleton<ITelegramClient, TelegramClient>();
        #else
            builder.Services.AddScoped<ITelegramClient, TelegramClient>();
        #endif

        builder.Services.AddTransient<ITelegramServiceProvider, TelegramServiceProvider>();
        builder.Services.AddTransient<IHtmlDocument, KdmidHtmlDocument>();
        builder.Services.AddTransient<IHttpClient, KdmidHttpClient>();
        builder.Services.AddTransient<ITelegramCommand, TelegramCommand>();
        builder.Services.AddTransient<ICaptchaService, AntiCaptchaService>();
        builder.Services.AddTransient<IKdmidService, KdmidService>();
    })
    .Build();

host.Run();
