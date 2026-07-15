namespace Semogly.Maestro.Abstractions.Activities;

public interface IActivityHandlerResolver
{
    /// <exception cref="ActivityHandlerNotFoundException">No handler is registered for <paramref name="activityType"/>.</exception>
    IActivityHandler Resolve(string activityType);
}
