using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Telegram.ApAzureBot.Core;
using Telegram.ApAzureBot.Core.Abstractions;
using Telegram.ApAzureBot.Infrastructure.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((context, builder) =>
    {
        builder.Services.AddLogging();
        builder.Services.AddHttpClient(Constants.Kdmid);
        builder.Services.AddSingleton<MemoryCache>();

        #if DEBUG
            builder.Services.AddSingleton<IClient, TelegramService>();
        #else
            builder.Services.AddScoped<ITelegramService, TelegramService>();
        #endif

        //builder.Services.AddTransient<IResponseService, ResponseService>();
        
        //builder.Services.AddTransient<IKdmidService, KdmidService>();
    })
    .Build();

host.Run();
