using System.Reflection;

using KdmidScheduler;
using KdmidScheduler.Infrastructure;

using Net.Shared.Background;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddKdmidInfrastructure()
    .AddKdmidVpsInfrastructure()
    .AddKdmidCore()
    .AddBackgroundServices(Assembly.GetExecutingAssembly());

builder
    .Build()
    .Run();
