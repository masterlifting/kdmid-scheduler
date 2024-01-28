using KdmidScheduler;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;
using KdmidScheduler.Infrastructure;
using KdmidScheduler.Worker.KdmidBackground.Tasks;

using Net.Shared.Background;
using Net.Shared.Persistence.Repositories.MongoDb;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddKdmidInfrastructure()
    .AddKdmidVpsInfrastructure()
    .AddKdmidCore()
    .AddBackgroundTask<KdmidBelgradeTask>(KdmidBelgradeTask.Name, x =>
    {
        x.AddStepsReaderRepository<KdmidAvailableDatesSteps, MongoDbReaderRepository<KdmidAvailableDatesSteps>>();
        x.AddProcessRepository<KdmidAvailableDates, MongoDbProcessRepository<KdmidAvailableDates>>();
    })
    .AddBackgroundTask<KdmidParisTask>(KdmidParisTask.Name, x =>
    {
        x.AddStepsReaderRepository<KdmidAvailableDatesSteps, MongoDbReaderRepository<KdmidAvailableDatesSteps>>();
        x.AddProcessRepository<KdmidAvailableDates, MongoDbProcessRepository<KdmidAvailableDates>>();
    })
    .AddBackgroundTask<KdmidBudapestTask>(KdmidBudapestTask.Name, x =>
    {
        x.AddStepsReaderRepository<KdmidAvailableDatesSteps, MongoDbReaderRepository<KdmidAvailableDatesSteps>>();
        x.AddProcessRepository<KdmidAvailableDates, MongoDbProcessRepository<KdmidAvailableDates>>();
    });

builder
    .Build()
    .Run();
