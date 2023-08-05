﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Telegram.ApAzureBot;
using Telegram.ApAzureBot.Services.Implementations;
using Telegram.ApAzureBot.Services.Interfaces;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((context, builder) =>
    {
        builder.Services.AddLogging();

        builder.Services.AddHttpClient(Constants.Kdmid, c =>
        {
            c.BaseAddress = new Uri("https://belgrad.kdmid.ru/queue/");
        });
        
        builder.Services.AddSingleton<MemoryCache>();
        
        //TODO: For production it should be replaced with Scoped
        builder.Services.AddSingleton<ITelegramService, TelegramService>();

        builder.Services.AddTransient<IResponseService, ResponseService>();
        
        builder.Services.AddTransient<IKdmidService, KdmidService>();
    })
    .Build();

host.Run();
