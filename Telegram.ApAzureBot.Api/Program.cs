using Microsoft.Extensions.Hosting;

using static Telegram.ApAzureBot.Infrastructure.Registrations;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((context, builder) => builder.Services.AddApAzureBotApiServices(context.Configuration))
    .Build();

host.Run();
