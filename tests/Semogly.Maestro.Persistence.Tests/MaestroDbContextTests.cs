using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Semogly.Maestro.Core.Entities;
using Semogly.Maestro.Core.Enums;

namespace Semogly.Maestro.Persistence.Tests;

public class MaestroDbContextTests
{
    private static MaestroDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<MaestroDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public void Model_BuildsWithoutErrors()
    {
        using var context = CreateContext();

        var act = () => context.Model.GetEntityTypes().ToList();

        act.Should().NotThrow();
    }

    [Fact]
    public async Task SavingFullExecutionGraph_PersistsAllLevels()
    {
        using var context = CreateContext();

        var activity = new Activity { Name = "Send Email", Type = "email" };
        var process = new Process { Name = "Onboarding" };
        var processFlow = new ProcessFlow { Name = "Daily Onboarding", CronParam = "0 * * * *" };

        var processActivity = new ProcessActivity { IdProcess = process.Id, IdActivity = activity.Id, Order = 1 };
        var processFlowProcess = new ProcessFlowProcess { IdProcessFlow = processFlow.Id, IdProcess = process.Id, Order = 1 };

        var flowExecution = new ProcessFlowExecution { IdProcessFlow = processFlow.Id, Status = ExecutionStatus.Running };
        var processExecution = new ProcessExecution
        {
            IdProcessFlowExecution = flowExecution.Id,
            IdProcessFlowProcess = processFlowProcess.Id,
            Status = ExecutionStatus.Running,
        };
        var activityExecution = new ActivityExecution
        {
            IdProcessExecution = processExecution.Id,
            IdProcessActivity = processActivity.Id,
            Status = ExecutionStatus.Completed,
        };

        context.AddRange(activity, process, processFlow, processActivity, processFlowProcess,
            flowExecution, processExecution, activityExecution);

        await context.SaveChangesAsync();

        (await context.ActivityExecutions.CountAsync()).Should().Be(1);
        (await context.ActivityExecutions.SingleAsync()).Status.Should().Be(ExecutionStatus.Completed);
    }
}
