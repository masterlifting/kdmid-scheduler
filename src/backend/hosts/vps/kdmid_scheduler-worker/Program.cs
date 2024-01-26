using KdmidScheduler;
using KdmidScheduler.Infrastructure;
using KdmidScheduler.Worker.KdmidBackground.Tasks;
using Net.Shared.Background;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddKdmidInfrastructure()
    .AddKdmidVpsInfrastructure()
    .AddKdmidCore()
    .AddBackgroundTask<KdmidBelgradeTask>(KdmidBelgradeTask.Name)
    .AddBackgroundTask<KdmidParisTask>(KdmidParisTask.Name)
    .AddBackgroundTask<KdmidBudapestTask>(KdmidBudapestTask.Name);

builder
    .Build()
    .Run();
