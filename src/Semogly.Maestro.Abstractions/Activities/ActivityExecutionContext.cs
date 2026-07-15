namespace Semogly.Maestro.Abstractions.Activities;

public sealed class ActivityExecutionContext
{
    public required Guid ActivityExecutionId { get; init; }

    public required Guid ProcessActivityId { get; init; }

    /// <summary>ActivityParam defaults merged with ProcessActivityParam overrides, keyed by ActivityParam.Key.</summary>
    public required IReadOnlyDictionary<string, string?> Parameters { get; init; }
}
