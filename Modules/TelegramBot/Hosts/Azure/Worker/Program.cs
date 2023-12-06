using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using static Telegram.ApAzureBot.Infrastructure.Registrations;

new HostBuilder()
    .ConfigureLogging(logger => logger.AddSimpleConsole())
    .ConfigureFunctionsWorkerDefaults((_, builder) => builder.Services.ConfigureWorker())
    .Build()
    .Run();
