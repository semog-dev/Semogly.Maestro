using Semogly.Maestro.Core.Enums;

namespace Semogly.Maestro.Core.Entities;

public class ProcessExecution : Entity
{
    public required Guid IdProcessFlowExecution { get; set; }

    public required Guid IdProcessFlowProcess { get; set; }

    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public string? Error { get; set; }

    public ProcessFlowExecution ProcessFlowExecution { get; init; } = null!;

    public ProcessFlowProcess ProcessFlowProcess { get; init; } = null!;

    public ICollection<ActivityExecution> ActivityExecutions { get; init; } = [];
}
