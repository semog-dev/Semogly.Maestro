namespace Semogly.Maestro.Abstractions.Activities;

/// <summary>Implemented by consumers to execute one Activity.Type. Throw to signal failure — the engine records the exception on ActivityExecution.Error. The returned value, if any, is stored on ActivityExecution.Output.</summary>
public interface IActivityHandler
{
    Task<string?> ExecuteAsync(ActivityExecutionContext context, CancellationToken cancellationToken);
}
