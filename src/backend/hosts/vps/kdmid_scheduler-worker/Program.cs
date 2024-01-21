using KdmidScheduler.Worker.KdmidAvailableDates;

using Net.Shared.Background;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddBackgroundService<KdmidAvailableDatesBackgroundService>();

builder
    .Build()
    .Run();
