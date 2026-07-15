namespace Semogly.Maestro.Abstractions.Activities;

public sealed class ActivityHandlerNotFoundException(string activityType)
    : Exception($"No activity handler registered for type '{activityType}'.")
{
    public string ActivityType { get; } = activityType;
}
