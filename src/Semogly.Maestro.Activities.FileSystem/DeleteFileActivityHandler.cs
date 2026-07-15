using Semogly.Maestro.Abstractions.Activities;

namespace Semogly.Maestro.Activities.FileSystem;

/// <summary>Register under <see cref="FileSystemActivityTypes.DeleteFile"/>. Params: Path (required). No-op if the file doesn't exist, matching File.Delete semantics.</summary>
public sealed class DeleteFileActivityHandler : IActivityHandler
{
    public Task<string?> ExecuteAsync(ActivityExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var path = context.GetRequired("Path");
        File.Delete(path);

        return Task.FromResult<string?>(null);
    }
}
