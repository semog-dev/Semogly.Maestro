namespace Semogly.Maestro.Core.Entities;

public class ProcessActivity : Entity
{
    public required Guid IdProcess { get; set; }

    public required Guid IdActivity { get; set; }

    /// <summary>Execution order of this activity within its parent process.</summary>
    public int Order { get; set; }

    public Process Process { get; init; } = null!;

    public Activity Activity { get; init; } = null!;

    public ICollection<ProcessActivityParam> Params { get; init; } = [];

    public ICollection<ActivityExecution> Executions { get; init; } = [];
}
