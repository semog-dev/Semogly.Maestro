using Semogly.Maestro.Core.Enums;

namespace Semogly.Maestro.Core.Entities;

public class ActivityExecution : Entity
{
    public required Guid IdProcessExecution { get; set; }

    public required Guid IdProcessActivity { get; set; }

    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;

    public int RetryCount { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public string? Error { get; set; }

    /// <summary>Whatever the handler chose to hand back on success (e.g. an HTTP response body). Null if the handler produced nothing.</summary>
    public string? Output { get; set; }

    public ProcessExecution ProcessExecution { get; init; } = null!;

    public ProcessActivity ProcessActivity { get; init; } = null!;
}
