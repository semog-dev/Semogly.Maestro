namespace Semogly.Maestro.Core.Entities;

public class ProcessActivityParam : Entity
{
    public required Guid IdProcessActivity { get; set; }

    public required string Key { get; set; }

    public string? Value { get; set; }

    public ProcessActivity ProcessActivity { get; init; } = null!;
}
