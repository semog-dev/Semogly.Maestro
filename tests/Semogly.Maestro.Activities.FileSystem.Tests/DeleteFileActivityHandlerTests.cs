using FluentAssertions;

namespace Semogly.Maestro.Activities.FileSystem.Tests;

public class DeleteFileActivityHandlerTests : TempDirectoryTests
{
    [Fact]
    public async Task DeletesExistingFile()
    {
        var path = Path.Combine(TempDir, "file.txt");
        await File.WriteAllTextAsync(path, "hello");

        await new DeleteFileActivityHandler().ExecuteAsync(TestContext.Create(("Path", path)), CancellationToken.None);

        File.Exists(path).Should().BeFalse();
    }

    [Fact]
    public async Task MissingFile_IsNoOp()
    {
        var path = Path.Combine(TempDir, "does-not-exist.txt");

        var act = () => new DeleteFileActivityHandler().ExecuteAsync(TestContext.Create(("Path", path)), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
