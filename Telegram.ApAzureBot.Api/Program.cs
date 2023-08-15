using Microsoft.Extensions.Hosting;

using static Telegram.ApAzureBot.Infrastructure.Registrations;

new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((_, builder) => builder.Services.ConfigureApi())
    .Build()
    .Run();
