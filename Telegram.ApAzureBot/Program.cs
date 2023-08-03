using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Telegram.ApAzureBot;
using Telegram.ApAzureBot.Services.Implementations;
using Telegram.ApAzureBot.Services.Interfaces;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((context, builder) =>
    {
        builder.Services.AddLogging();

        builder.Services.AddHttpClient(Constants.MidRfHttpClientName, c =>
        {
            c.BaseAddress = new Uri("https://belgrad.kdmid.ru/queue/");
        });
        builder.Services.AddTransient<IWebService, WebService>();
        builder.Services.AddTransient<ITelegramService, TelegramService>();
        builder.Services.AddTransient<IResponseService, ResponseService>();
    })
    .Build();

host.Run();
