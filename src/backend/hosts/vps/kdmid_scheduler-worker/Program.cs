using KdmidScheduler;
using KdmidScheduler.Infrastructure;
using KdmidScheduler.Worker.KdmidAvailableDatesBackground;

using Net.Shared.Background;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddKdmidInfrastructure()
    .AddKdmidVpsInfrastructure()
    .AddKdmidCore()
    .AddBackgroundService<KdmidAvailableDatesBackgroundService>();

builder
    .Build()
    .Run();
