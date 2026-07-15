using FluentAssertions;

namespace Semogly.Maestro.Activities.FileSystem.Tests;

public class FileSystemActivityDefinitionsTests
{
    [Fact]
    public void MoveFile_HasExpectedParams()
    {
        var activity = FileSystemActivityDefinitions.MoveFile();

        activity.Type.Should().Be(FileSystemActivityTypes.MoveFile);
        activity.Params.Select(p => p.Key).Should().BeEquivalentTo(["SourcePath", "DestinationPath", "OverwriteExisting"]);
        activity.Params.Single(p => p.Key == "SourcePath").Required.Should().BeTrue();
        activity.Params.Single(p => p.Key == "OverwriteExisting").DefaultValue.Should().Be("false");
    }

    [Fact]
    public void CopyFile_HasExpectedParams()
    {
        var activity = FileSystemActivityDefinitions.CopyFile();

        activity.Type.Should().Be(FileSystemActivityTypes.CopyFile);
        activity.Params.Select(p => p.Key).Should().BeEquivalentTo(["SourcePath", "DestinationPath", "OverwriteExisting"]);
    }

    [Fact]
    public void DeleteFile_HasExpectedParams()
    {
        var activity = FileSystemActivityDefinitions.DeleteFile();

        activity.Type.Should().Be(FileSystemActivityTypes.DeleteFile);
        activity.Params.Should().ContainSingle(p => p.Key == "Path" && p.Required);
    }
}
