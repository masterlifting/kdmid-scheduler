using KdmidScheduler.Infrastructure;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using static KdmidScheduler.Registrations;

new HostBuilder()
    .ConfigureLogging(logger => logger.AddSimpleConsole())
    .ConfigureFunctionsWorkerDefaults((_, builder) =>
    {
        builder.Services
            .AddKdmidInfrastructure()
            .AddKdmidAzureInfrastructure()
            .AddKdmidCore();
    })
    .Build()
    .Run();
