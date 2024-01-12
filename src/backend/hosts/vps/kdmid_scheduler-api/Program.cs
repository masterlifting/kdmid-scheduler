using KdmidScheduler.Infrastructure;

using static KdmidScheduler.Registrations;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddKdmidInfrastructure()
    .AddKdmidVpsInfrastructure()
    .AddKdmidCore()
    .AddControllers();

var app = builder.Build();

app
    .UseRouting()
    .UseCors(Constants.TelegramWebAppCorsPolicy);

app.MapControllers();

app.Run();
