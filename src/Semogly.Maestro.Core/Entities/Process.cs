namespace Semogly.Maestro.Core.Entities;

public class Process : Entity
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool Active { get; set; }

    /// <summary>When an Activity fails, whether the engine still runs the remaining Activities in this process.</summary>
    public bool ContinueOnActivityFailure { get; set; }

    public ICollection<ProcessFlowProcess> ProcessFlows { get; init; } = [];

    public ICollection<ProcessActivity> Activities { get; init; } = [];
}
