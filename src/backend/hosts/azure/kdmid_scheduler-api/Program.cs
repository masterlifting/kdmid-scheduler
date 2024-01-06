using KdmidScheduler.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using KdmidScheduler.Infrastructure;
using Net.Shared.Bots;
using KdmidScheduler.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

new HostBuilder()
    .ConfigureLogging(logger => logger.AddSimpleConsole())
    .ConfigureFunctionsWorkerDefaults((_, builder) =>
    {
        builder.Services
        .AddTransient<IKdmidService, KdmidService>()
        .AddKdmidInfrastructure()
        .AddTelegramBot(x =>
        {
            x.AddRequestHandler<KdmidBotRequestService>();
            x.AddResponseHandler<KdmidBotResponseService>();
        });
    })
    .Build()
    .Run();
