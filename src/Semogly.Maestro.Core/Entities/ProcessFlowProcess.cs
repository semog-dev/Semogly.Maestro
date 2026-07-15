namespace Semogly.Maestro.Core.Entities;

public class ProcessFlowProcess : Entity
{
    public required Guid IdProcessFlow { get; set; }

    public required Guid IdProcess { get; set; }

    /// <summary>Execution order of this process within its parent flow.</summary>
    public int Order { get; set; }

    /// <summary>Cron expression overriding the parent flow's schedule for this step, if set.</summary>
    public string? CronParam { get; set; }

    public ProcessFlow ProcessFlow { get; init; } = null!;

    public Process Process { get; init; } = null!;

    public ICollection<ProcessExecution> Executions { get; init; } = [];
}
