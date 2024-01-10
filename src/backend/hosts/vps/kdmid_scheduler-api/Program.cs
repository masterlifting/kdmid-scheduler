using System.Text.Json;

using KdmidScheduler.Infrastructure;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

using static KdmidScheduler.Registrations;

var builder = WebApplication.CreateSlimBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80);
});

builder.Services
    .AddKdmidInfrastructure()
    .AddKdmidVpsInfrastructure()
    .AddKdmidCore();

var app = builder.Build();

app.UseCors(Constants.TelegramWebAppCorsPolicy);

app.MapGet("/start", (IBotClient client, CancellationToken cToken) =>
{
    return client.Listen(cToken);
});
app.MapGet("/listen", (IBotClient client, HttpRequest request, CancellationToken cToken) =>
{
    var uri = new Uri($"{request.Scheme}://{request.Host}/receive");

    return client.Listen(uri, cToken);
});
app.MapPost("/receive", async (IBotClient client, HttpRequest request, CancellationToken cToken) =>
{
    using var reader = new StreamReader(request.Body);

    var data = await reader.ReadToEndAsync(cToken);

    ArgumentNullException.ThrowIfNull(data, "Received data is empty.");

    await client.Receive(data, cToken);
});
app.MapGet($"/chats/{{chatId}}/commands/{{commandId}}", async (IBotCommandsStore commandStore, string chatId, string commandId, CancellationToken cToken) =>
{
    return await commandStore.Get(chatId, new Guid(commandId), cToken);
});
app.MapPost($"/chats/{{chatId}}", async (IBotCommandsStore commandStore, IBotResponse responseService, HttpRequest request, string chatId, CancellationToken cToken) =>
{
    using var reader = new StreamReader(request.Body);

    var data = await reader.ReadToEndAsync(cToken);

    ArgumentNullException.ThrowIfNull(data, "Received data is empty.");

    var command = JsonSerializer.Deserialize<BotCommand>(data, options: new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    if (command is null || command.Id == Guid.Empty)
        throw new ArgumentNullException(nameof(command), "Received command is empty.");

    await commandStore.Update(chatId, command.Id, command, cToken);

    await responseService.Create(chatId, command, cToken);
});

app.Run();
