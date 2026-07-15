namespace Semogly.Maestro.Activities.FileSystem.Tests;

public abstract class TempDirectoryTests : IDisposable
{
    protected string TempDir { get; } =
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;

    public void Dispose() => Directory.Delete(TempDir, recursive: true);
}
