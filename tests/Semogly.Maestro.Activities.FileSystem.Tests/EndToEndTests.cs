using FluentAssertions;
using Semogly.Maestro.Abstractions.Activities;
using Semogly.Maestro.Core.Entities;
using Semogly.Maestro.Core.Enums;
using Semogly.Maestro.Engine;

namespace Semogly.Maestro.Activities.FileSystem.Tests;

public class EndToEndTests : TempDirectoryTests
{
    private sealed class Resolver(IReadOnlyDictionary<string, IActivityHandler> handlers) : IActivityHandlerResolver
    {
        public IActivityHandler Resolve(string activityType) => handlers[activityType];
    }

    [Fact]
    public async Task MoveFile_RunsThroughProcessFlowExecutor()
    {
        var source = Path.Combine(TempDir, "source.txt");
        var destination = Path.Combine(TempDir, "archive", "destination.txt");
        await File.WriteAllTextAsync(source, "payload");

        var activity = FileSystemActivityDefinitions.MoveFile();
        var process = new Process { Name = "Archive" };
        var processActivity = new ProcessActivity
        {
            IdProcess = process.Id, IdActivity = activity.Id, Order = 1, Process = process, Activity = activity,
        };
        processActivity.Params.Add(new ProcessActivityParam { IdProcessActivity = processActivity.Id, Key = "SourcePath", Value = source });
        processActivity.Params.Add(new ProcessActivityParam { IdProcessActivity = processActivity.Id, Key = "DestinationPath", Value = destination });
        process.Activities.Add(processActivity);

        var flow = new ProcessFlow { Name = "Nightly Archive" };
        var link = new ProcessFlowProcess { IdProcessFlow = flow.Id, IdProcess = process.Id, Order = 1, ProcessFlow = flow, Process = process };
        flow.Processes.Add(link);

        var resolver = new Resolver(new Dictionary<string, IActivityHandler>
        {
            [FileSystemActivityTypes.MoveFile] = new MoveFileActivityHandler(),
        });
        var executor = new ProcessFlowExecutor(resolver);

        var result = await executor.ExecuteAsync(flow);

        result.Status.Should().Be(ExecutionStatus.Completed);
        File.Exists(destination).Should().BeTrue();
        File.Exists(source).Should().BeFalse();
    }
}
