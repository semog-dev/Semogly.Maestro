namespace Semogly.Maestro.Core.Entities;

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
