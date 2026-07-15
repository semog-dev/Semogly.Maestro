using FluentAssertions;

namespace Semogly.Maestro.Activities.FileSystem.Tests;

public class CopyFileActivityHandlerTests : TempDirectoryTests
{
    [Fact]
    public async Task CopiesFile_KeepingSource()
    {
        var source = Path.Combine(TempDir, "source.txt");
        var destination = Path.Combine(TempDir, "destination.txt");
        await File.WriteAllTextAsync(source, "hello");

        await new CopyFileActivityHandler().ExecuteAsync(
            TestContext.Create(("SourcePath", source), ("DestinationPath", destination)), CancellationToken.None);

        File.Exists(source).Should().BeTrue();
        (await File.ReadAllTextAsync(destination)).Should().Be("hello");
    }

    [Fact]
    public async Task CreatesDestinationDirectory_WhenMissing()
    {
        var source = Path.Combine(TempDir, "source.txt");
        var destination = Path.Combine(TempDir, "nested", "destination.txt");
        await File.WriteAllTextAsync(source, "hello");

        await new CopyFileActivityHandler().ExecuteAsync(
            TestContext.Create(("SourcePath", source), ("DestinationPath", destination)), CancellationToken.None);

        File.Exists(destination).Should().BeTrue();
    }

    [Fact]
    public async Task ExistingDestination_WithoutOverwrite_Throws()
    {
        var source = Path.Combine(TempDir, "source.txt");
        var destination = Path.Combine(TempDir, "destination.txt");
        await File.WriteAllTextAsync(source, "new");
        await File.WriteAllTextAsync(destination, "old");

        var act = () => new CopyFileActivityHandler().ExecuteAsync(
            TestContext.Create(("SourcePath", source), ("DestinationPath", destination)), CancellationToken.None);

        await act.Should().ThrowAsync<IOException>();
    }
}
