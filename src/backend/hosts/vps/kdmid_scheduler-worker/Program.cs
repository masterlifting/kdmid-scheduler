using KdmidScheduler;
using KdmidScheduler.Infrastructure;
using KdmidScheduler.Worker.KdmidBackground.Tasks;
using Net.Shared.Background;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddKdmidInfrastructure()
    .AddKdmidVpsInfrastructure()
    .AddKdmidCore()
    .AddBackgroundTask<KdmidBelgradeTask>("Belgrade")
    .AddBackgroundTask<KdmidParisTask>("Paris")
    .AddBackgroundTask<KdmidBudapestTask>("Budapest");

builder
    .Build()
    .Run();
