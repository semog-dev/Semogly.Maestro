using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Semogly.Maestro.Abstractions.Activities;

namespace Semogly.Maestro.Engine;

public static class EngineServiceCollectionExtensions
{
    /// <summary>Registers IProcessFlowExecutor and its default IActivityHandlerResolver. Register activity handlers separately (e.g. via an Activities.* package's own AddXxxActivities extension) before resolving the executor.</summary>
    public static IServiceCollection AddMaestroEngine(this IServiceCollection services)
    {
        services.AddActivityHandlerResolver();
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IProcessFlowExecutor, ProcessFlowExecutor>();
        return services;
    }
}
