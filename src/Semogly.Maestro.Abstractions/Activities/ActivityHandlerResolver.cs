using Microsoft.Extensions.DependencyInjection;

namespace Semogly.Maestro.Abstractions.Activities;

/// <summary>Default IActivityHandlerResolver, backed by keyed DI services registered via AddActivityHandler.</summary>
internal sealed class ActivityHandlerResolver(IServiceProvider serviceProvider) : IActivityHandlerResolver
{
    public IActivityHandler Resolve(string activityType) =>
        serviceProvider.GetKeyedService<IActivityHandler>(activityType)
            ?? throw new ActivityHandlerNotFoundException(activityType);
}
