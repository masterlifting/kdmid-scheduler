using KdmidScheduler;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;
using KdmidScheduler.Infrastructure;
using KdmidScheduler.Infrastructure.Persistence.Contexts;
using KdmidScheduler.Worker.KdmidBackground.Tasks;

using Net.Shared.Background;
using Net.Shared.Persistence.Repositories.MongoDb;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddKdmidCore()
    .AddKdmidVpsInfrastructure()
    .AddBackgroundTasks(x =>
    {
        x.AddProcessRepository<KdmidAvailableDates, MongoDbProcessRepository<KdmidMongoDbContext, KdmidAvailableDates>>();
        x.AddProcessStepsRepository<KdmidAvailableDatesSteps, MongoDbReaderRepository<KdmidMongoDbContext, KdmidAvailableDatesSteps>>();

        x.AddTask<KdmidBelgradeTask>(KdmidBelgradeTask.Name);
        x.AddTask<KdmidBerlinTask>(KdmidBerlinTask.Name);
        x.AddTask<KdmidBernTask>(KdmidBernTask.Name);
        x.AddTask<KdmidBrusselsTask>(KdmidBrusselsTask.Name);
        x.AddTask<KdmidBudapestTask>(KdmidBudapestTask.Name);
        x.AddTask<KdmidHagueTask>(KdmidHagueTask.Name);
        x.AddTask<KdmidDublinTask>(KdmidDublinTask.Name);
        x.AddTask<KdmidHelsinkiTask>(KdmidHelsinkiTask.Name);
        x.AddTask<KdmidLjubljanaTask>(KdmidLjubljanaTask.Name);
        x.AddTask<KdmidParisTask>(KdmidParisTask.Name);
        x.AddTask<KdmidPodgoricaTask>(KdmidPodgoricaTask.Name);
        x.AddTask<KdmidRigaTask>(KdmidRigaTask.Name);
        x.AddTask<KdmidSarajevoTask>(KdmidSarajevoTask.Name);
        x.AddTask<KdmidTiranaTask>(KdmidTiranaTask.Name);
    });

builder
    .Build()
    .Run();
