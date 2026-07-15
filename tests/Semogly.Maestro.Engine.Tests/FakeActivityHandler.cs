using Semogly.Maestro.Abstractions.Activities;

namespace Semogly.Maestro.Engine.Tests;

internal sealed class FakeActivityHandler(Func<ActivityExecutionContext, Task<string?>>? execute = null) : IActivityHandler
{
    public int CallCount { get; private set; }

    public IReadOnlyDictionary<string, string?>? LastParameters { get; private set; }

    public Task<string?> ExecuteAsync(ActivityExecutionContext context, CancellationToken cancellationToken)
    {
        CallCount++;
        LastParameters = context.Parameters;
        return execute?.Invoke(context) ?? Task.FromResult<string?>(null);
    }
}
