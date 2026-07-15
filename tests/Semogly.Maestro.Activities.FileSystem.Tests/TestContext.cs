using Semogly.Maestro.Abstractions.Activities;

namespace Semogly.Maestro.Activities.FileSystem.Tests;

internal static class TestContext
{
    public static ActivityExecutionContext Create(params (string Key, string? Value)[] parameters) =>
        new()
        {
            ActivityExecutionId = Guid.NewGuid(),
            ProcessActivityId = Guid.NewGuid(),
            Parameters = parameters.ToDictionary(p => p.Key, p => p.Value),
        };
}
