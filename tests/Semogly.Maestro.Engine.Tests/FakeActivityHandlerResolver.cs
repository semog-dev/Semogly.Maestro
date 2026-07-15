using Semogly.Maestro.Abstractions.Activities;

namespace Semogly.Maestro.Engine.Tests;

internal sealed class FakeActivityHandlerResolver : IActivityHandlerResolver
{
    private readonly Dictionary<string, IActivityHandler> _handlers = [];

    public FakeActivityHandlerResolver Register(string activityType, IActivityHandler handler)
    {
        _handlers[activityType] = handler;
        return this;
    }

    public IActivityHandler Resolve(string activityType) =>
        _handlers.TryGetValue(activityType, out var handler)
            ? handler
            : throw new ActivityHandlerNotFoundException(activityType);
}
