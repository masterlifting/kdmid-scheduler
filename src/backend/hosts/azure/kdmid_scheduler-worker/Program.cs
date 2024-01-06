using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

new HostBuilder()
    .ConfigureLogging(logger => logger.AddSimpleConsole())
    .ConfigureFunctionsWorkerDefaults((_, builder) => builder.Services.ConfigureWorker())
    .Build()
    .Run();
