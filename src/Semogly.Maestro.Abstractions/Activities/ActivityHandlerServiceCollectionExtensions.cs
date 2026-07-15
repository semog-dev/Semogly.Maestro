using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Semogly.Maestro.Abstractions.Activities;

public static class ActivityHandlerServiceCollectionExtensions
{
    /// <summary>Registers the default IActivityHandlerResolver, which resolves handlers added via AddActivityHandler by their Activity.Type key.</summary>
    public static IServiceCollection AddActivityHandlerResolver(this IServiceCollection services)
    {
        services.TryAddSingleton<IActivityHandlerResolver, ActivityHandlerResolver>();
        return services;
    }

    /// <summary>Registers THandler as the IActivityHandler for activityType, resolvable through IActivityHandlerResolver.</summary>
    public static IServiceCollection AddActivityHandler<THandler>(this IServiceCollection services, string activityType)
        where THandler : class, IActivityHandler
    {
        services.AddKeyedTransient<IActivityHandler, THandler>(activityType);
        return services;
    }

    /// <summary>Registers a handler built by <paramref name="factory"/> as the IActivityHandler for activityType — for handlers whose dependencies need custom wiring (e.g. a typed HttpClient).</summary>
    public static IServiceCollection AddActivityHandler(this IServiceCollection services, string activityType, Func<IServiceProvider, IActivityHandler> factory)
    {
        services.AddKeyedTransient(activityType, (sp, _) => factory(sp));
        return services;
    }
}
