using KdmidScheduler;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;
using KdmidScheduler.Infrastructure;
using KdmidScheduler.Infrastructure.Persistence.MongoDb.Contexts;
using KdmidScheduler.Worker.Background.Tasks;

using Net.Shared.Background;
using Net.Shared.Persistence.Repositories.MongoDb;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddKdmidCore()
    .AddKdmidVpsInfrastructure()
    .AddBackgroundTasks(x =>
    {
        x.AddProcessRepository<KdmidAvailableDates, MongoDbProcessRepository<KdmidPersistenceContext, KdmidAvailableDates>>();
        x.AddProcessStepsRepository<KdmidAvailableDatesSteps, MongoDbReaderRepository<KdmidPersistenceContext, KdmidAvailableDatesSteps>>();

        x.AddTask<Belgrade>(Belgrade.Name);
        x.AddTask<Berlin>(Berlin.Name);
        x.AddTask<Bern>(Bern.Name);
        x.AddTask<Brussels>(Brussels.Name);
        x.AddTask<Budapest>(Budapest.Name);
        x.AddTask<Hague>(Hague.Name);
        x.AddTask<Dublin>(Dublin.Name);
        x.AddTask<Helsinki>(Helsinki.Name);
        x.AddTask<Ljubljana>(Ljubljana.Name);
        x.AddTask<Paris>(Paris.Name);
        x.AddTask<Podgorica>(Podgorica.Name);
        x.AddTask<Riga>(Riga.Name);
        x.AddTask<Sarajevo>(Sarajevo.Name);
        x.AddTask<Tirana>(Tirana.Name);
    });

builder
    .Build()
    .Run();
