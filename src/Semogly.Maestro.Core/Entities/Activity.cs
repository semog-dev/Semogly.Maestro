namespace Semogly.Maestro.Core.Entities;

public class Activity : Entity
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool Active { get; set; }

    /// <summary>Key matched against a registered activity handler at execution time.</summary>
    public required string Type { get; set; }

    /// <summary>Extra attempts the engine makes, immediately and in-process, after the first failure. 0 means no retry.</summary>
    public int MaxRetryCount { get; set; }

    public ICollection<ActivityParam> Params { get; init; } = [];

    public ICollection<ProcessActivity> ProcessActivities { get; init; } = [];
}
