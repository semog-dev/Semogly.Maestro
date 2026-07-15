namespace Semogly.Maestro.Core.Entities;

public class ProcessFlow : Entity
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool Active { get; set; }

    /// <summary>Cron expression controlling when this flow is scheduled to run.</summary>
    public string? CronParam { get; set; }

    /// <summary>When a Process fails, whether the engine still runs the remaining Processes in this flow.</summary>
    public bool ContinueOnProcessFailure { get; set; }

    public ICollection<ProcessFlowProcess> Processes { get; init; } = [];

    public ICollection<ProcessFlowExecution> Executions { get; init; } = [];
}
