using Semogly.Maestro.Core.Enums;

namespace Semogly.Maestro.Core.Entities;

public class ProcessFlowExecution : Entity
{
    public required Guid IdProcessFlow { get; set; }

    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public string? Error { get; set; }

    public ProcessFlow ProcessFlow { get; init; } = null!;

    public ICollection<ProcessExecution> ProcessExecutions { get; init; } = [];
}
