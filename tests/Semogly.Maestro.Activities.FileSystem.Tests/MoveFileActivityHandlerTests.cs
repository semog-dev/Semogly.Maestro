using FluentAssertions;

namespace Semogly.Maestro.Activities.FileSystem.Tests;

public class MoveFileActivityHandlerTests : TempDirectoryTests
{
    [Fact]
    public async Task MovesFile_FromSourceToDestination()
    {
        var source = Path.Combine(TempDir, "source.txt");
        var destination = Path.Combine(TempDir, "destination.txt");
        await File.WriteAllTextAsync(source, "hello");

        await new MoveFileActivityHandler().ExecuteAsync(
            TestContext.Create(("SourcePath", source), ("DestinationPath", destination)), CancellationToken.None);

        File.Exists(source).Should().BeFalse();
        (await File.ReadAllTextAsync(destination)).Should().Be("hello");
    }

    [Fact]
    public async Task CreatesDestinationDirectory_WhenMissing()
    {
        var source = Path.Combine(TempDir, "source.txt");
        var destination = Path.Combine(TempDir, "nested", "destination.txt");
        await File.WriteAllTextAsync(source, "hello");

        await new MoveFileActivityHandler().ExecuteAsync(
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

        var act = () => new MoveFileActivityHandler().ExecuteAsync(
            TestContext.Create(("SourcePath", source), ("DestinationPath", destination)), CancellationToken.None);

        await act.Should().ThrowAsync<IOException>();
    }

    [Fact]
    public async Task ExistingDestination_WithOverwrite_Replaces()
    {
        var source = Path.Combine(TempDir, "source.txt");
        var destination = Path.Combine(TempDir, "destination.txt");
        await File.WriteAllTextAsync(source, "new");
        await File.WriteAllTextAsync(destination, "old");

        await new MoveFileActivityHandler().ExecuteAsync(
            TestContext.Create(("SourcePath", source), ("DestinationPath", destination), ("OverwriteExisting", "true")),
            CancellationToken.None);

        (await File.ReadAllTextAsync(destination)).Should().Be("new");
    }
}
