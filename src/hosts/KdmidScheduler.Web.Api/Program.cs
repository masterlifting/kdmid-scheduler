using System.Globalization;

using KdmidScheduler.Abstractions.Interfaces;
using KdmidScheduler.Infrastructure;
using KdmidScheduler.Services;

using Net.Shared.Bots;
using Net.Shared.Bots.Abstractions.Interfaces;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
        .AddTransient<IKdmidService, KdmidService>()
        .AddKdmidInfrastructure()
        .AddTelegramBot<KdmidBotRequestService, KdmidBotResponseService>();

var app = builder.Build();

app.MapGet("/listen", (IBotClient client, HttpRequest request, CancellationToken cToken) =>
{
    var url = request.QueryString.Value?.Replace("/listen", "/receive", true, CultureInfo.InvariantCulture);

    ArgumentNullException.ThrowIfNull(url, "Received url is empty.");

    var uri = new Uri(url);

    return client.Listen(uri, cToken);
});

app.MapPost("/receive", async (IBotClient client, HttpRequest request, CancellationToken cToken) =>
{
    using var reader = new StreamReader(request.Body);
    var data = await reader.ReadToEndAsync(cToken);
    
    ArgumentNullException.ThrowIfNull(data, "Received data is empty.");

    await client.Receive(data, cToken);
});

app.Run();
