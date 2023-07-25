using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Telegram.ApAzureBot.Services.Implementations;
using Telegram.ApAzureBot.Services.Interfaces;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((context,builder) =>
    {
        builder.Services.AddTransient<ITelegramService, TelegramService>();
        builder.Services.AddTransient<IResponseService, ResponseService>();
    })
    .Build();

host.Run();
