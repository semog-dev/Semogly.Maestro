using Semogly.Maestro.Abstractions.Activities;

namespace Semogly.Maestro.Activities.FileSystem;

/// <summary>Register under <see cref="FileSystemActivityTypes.CopyFile"/>. Params: SourcePath (required), DestinationPath (required), OverwriteExisting (optional, default false).</summary>
public sealed class CopyFileActivityHandler : IActivityHandler
{
    public Task<string?> ExecuteAsync(ActivityExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sourcePath = context.GetRequired("SourcePath");
        var destinationPath = context.GetRequired("DestinationPath");
        var overwrite = context.GetBoolean("OverwriteExisting");

        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(destinationDirectory))
            Directory.CreateDirectory(destinationDirectory);

        File.Copy(sourcePath, destinationPath, overwrite);

        return Task.FromResult<string?>(null);
    }
}
