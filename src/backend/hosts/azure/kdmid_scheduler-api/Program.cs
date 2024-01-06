using KdmidScheduler.Abstractions.Interfaces.Services;
using KdmidScheduler.Infrastructure;
using KdmidScheduler.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

new HostBuilder()
    .ConfigureLogging(logger => logger.AddSimpleConsole())
    .ConfigureFunctionsWorkerDefaults((_, builder) =>
    {
        builder.Services
            .AddKdmidAzureInfrastructure()
            .AddTransient<IKdmidService, KdmidService>();
    })
    .Build()
    .Run();
