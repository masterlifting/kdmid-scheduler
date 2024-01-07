using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Services;

using Microsoft.Extensions.DependencyInjection;

namespace KdmidScheduler;

public static class Registrations
{
    public static IServiceCollection AddKdmidCore(this IServiceCollection services) => services
        .AddTransient<IKdmidRequestService, KdmidRequestService>()
        .AddTransient<IKdmidResponseService, KdmidResponseService>();
}
