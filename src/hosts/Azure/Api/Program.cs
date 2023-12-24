using KdmidScheduler.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Net.Shared.Bots;

using TelegramBot.Abstractions.Interfaces.Services.Kdmid;
using TelegramBot.Services;

new HostBuilder()
    .ConfigureLogging(logger => logger.AddSimpleConsole())
    .ConfigureFunctionsWorkerDefaults((_, builder) =>
    {
        builder.Services.AddTelegramBot<KdmidBotService>();
        builder.Services.AddTransient<IKdmidService, KdmidService>();
    })
    .Build()
    .Run();
