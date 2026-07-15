namespace Semogly.Maestro.Core.Entities;

public class ActivityParam : Entity
{
    public required Guid IdActivity { get; set; }

    public required string Key { get; set; }

    public bool Required { get; set; }

    public string? DefaultValue { get; set; }

    public Activity Activity { get; init; } = null!;
}
