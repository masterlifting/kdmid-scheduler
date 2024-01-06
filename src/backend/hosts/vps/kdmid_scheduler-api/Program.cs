using System.Text.Json;

using KdmidScheduler.Abstractions.Interfaces;
using KdmidScheduler.Infrastructure;
using KdmidScheduler.Infrastructure.Persistence.Repositories;
using KdmidScheduler.Services;

using Net.Shared.Bots;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddKdmidInfrastructure()
    .AddTransient<IKdmidService, KdmidService>()
    .AddTelegramBot(x =>
    {
        x.AddRequestHandler<KdmidBotRequestService>();
        x.AddResponseHandler<KdmidBotResponseService>();
        x.AddCommandsStore<KdmidBotCommandsStore>();
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("TelegramWebAppCorsPolicy", builder =>
        {
            builder
                .WithOrigins("http://localhost:3000")
                .WithMethods("GET", "POST")
                .AllowCredentials()
                .WithHeaders("Content-Type");
        });
    });

var app = builder.Build();

app.UseCors("TelegramWebAppCorsPolicy");

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
app.MapPost($"/chats/{{chatId}}", async (IBotCommandsStore commandStore, IBotResponseService responseService, HttpRequest request, string chatId, CancellationToken cToken) =>
{
    using var reader = new StreamReader(request.Body);

    var data = await reader.ReadToEndAsync(cToken);

    ArgumentNullException.ThrowIfNull(data, "Received data is empty.");

    var command = JsonSerializer.Deserialize<BotCommand>(data, options: new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    if(command is null || command.Id == Guid.Empty)
    {
        throw new ArgumentNullException(nameof(command), "Received command is empty.");
    }

    await commandStore.Update(chatId, command.Id, command, cToken);

    await responseService.CreateResponse(chatId, command, cToken);
});

app.Run();
