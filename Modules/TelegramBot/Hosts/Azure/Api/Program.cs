using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using TelegramBot.Infrastructure;

using static TelegramBot.Infrastructure.Registrations;

new HostBuilder()
    .ConfigureLogging(logger => logger.AddSimpleConsole())
    .ConfigureFunctionsWorkerDefaults((_, builder) => builder.Services.ConfigureApi())
    .Build()
    .Run();
