using KdmidScheduler.Api;
using KdmidScheduler.Infrastructure;

using static KdmidScheduler.Registrations;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddKdmidCore()
    .AddKdmidVpsInfrastructure()
    .AddControllers(options => options.Filters.Add<BotExceptionFilter>());

builder.Services.AddCors(options =>
{
    options.AddPolicy(Constants.TelegramWebAppCorsPolicy, builder =>
    {
        builder
            .WithOrigins("https://kdmidweb.masterlifting.guru", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app
    .UseRouting()
    .UseCors(Constants.TelegramWebAppCorsPolicy);

app.MapControllers();

app.Run();
