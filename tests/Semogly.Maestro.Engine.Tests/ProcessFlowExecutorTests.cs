using FluentAssertions;
using Semogly.Maestro.Core.Entities;
using Semogly.Maestro.Core.Enums;
using static Semogly.Maestro.Engine.Tests.GraphBuilder;

namespace Semogly.Maestro.Engine.Tests;

public class ProcessFlowExecutorTests
{
    [Fact]
    public async Task SingleActivity_Succeeds()
    {
        var activity = NewActivity("noop");
        var process = NewProcess();
        process.AddActivity(activity, order: 1);
        var flow = NewFlow();
        flow.AddProcess(process, order: 1);

        var handler = new FakeActivityHandler();
        var resolver = new FakeActivityHandlerResolver().Register("noop", handler);
        var executor = new ProcessFlowExecutor(resolver);

        var result = await executor.ExecuteAsync(flow);

        result.Status.Should().Be(ExecutionStatus.Completed);
        result.ProcessExecutions.Should().ContainSingle().Which.Status.Should().Be(ExecutionStatus.Completed);
        result.ProcessExecutions.Single().ActivityExecutions.Should()
            .ContainSingle().Which.Status.Should().Be(ExecutionStatus.Completed);
        handler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task SuccessfulActivity_CapturesHandlerOutput()
    {
        var activity = NewActivity("noop");
        var process = NewProcess();
        process.AddActivity(activity, order: 1);
        var flow = NewFlow();
        flow.AddProcess(process, order: 1);

        var handler = new FakeActivityHandler(_ => Task.FromResult<string?>("hello"));
        var resolver = new FakeActivityHandlerResolver().Register("noop", handler);
        var executor = new ProcessFlowExecutor(resolver);

        var result = await executor.ExecuteAsync(flow);

        result.ProcessExecutions.Single().ActivityExecutions.Single().Output.Should().Be("hello");
    }

    [Fact]
    public async Task MissingRequiredParam_FailsWithoutCallingHandler()
    {
        var activity = NewActivity("send-email");
        activity.AddParam("to", required: true);
        var process = NewProcess();
        process.AddActivity(activity, order: 1);
        var flow = NewFlow();
        flow.AddProcess(process, order: 1);

        var handler = new FakeActivityHandler();
        var resolver = new FakeActivityHandlerResolver().Register("send-email", handler);
        var executor = new ProcessFlowExecutor(resolver);

        var result = await executor.ExecuteAsync(flow);

        var activityExecution = result.ProcessExecutions.Single().ActivityExecutions.Single();
        activityExecution.Status.Should().Be(ExecutionStatus.Failed);
        activityExecution.Error.Should().Contain("to");
        handler.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task ParamOverride_TakesPrecedenceOverDefault()
    {
        var activity = NewActivity("send-email");
        activity.AddParam("to", required: true, defaultValue: "default@example.com");
        var process = NewProcess();
        var link = process.AddActivity(activity, order: 1);
        link.SetParam("to", "override@example.com");
        var flow = NewFlow();
        flow.AddProcess(process, order: 1);

        var handler = new FakeActivityHandler();
        var resolver = new FakeActivityHandlerResolver().Register("send-email", handler);
        var executor = new ProcessFlowExecutor(resolver);

        await executor.ExecuteAsync(flow);

        handler.LastParameters.Should().ContainKey("to").WhoseValue.Should().Be("override@example.com");
    }

    [Fact]
    public async Task ActivityFailure_WithContinueDisabled_SkipsRemainingActivities()
    {
        var failing = NewActivity("failing");
        var next = NewActivity("next");
        var process = NewProcess(continueOnActivityFailure: false);
        process.AddActivity(failing, order: 1);
        process.AddActivity(next, order: 2);
        var flow = NewFlow();
        flow.AddProcess(process, order: 1);

        var failingHandler = new FakeActivityHandler(_ => throw new InvalidOperationException("boom"));
        var nextHandler = new FakeActivityHandler();
        var resolver = new FakeActivityHandlerResolver()
            .Register("failing", failingHandler)
            .Register("next", nextHandler);
        var executor = new ProcessFlowExecutor(resolver);

        var result = await executor.ExecuteAsync(flow);

        var processExecution = result.ProcessExecutions.Single();
        processExecution.Status.Should().Be(ExecutionStatus.Failed);
        processExecution.ActivityExecutions.Should().ContainSingle();
        nextHandler.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task ActivityFailure_WithContinueEnabled_RunsRemainingActivities()
    {
        var failing = NewActivity("failing");
        var next = NewActivity("next");
        var process = NewProcess(continueOnActivityFailure: true);
        process.AddActivity(failing, order: 1);
        process.AddActivity(next, order: 2);
        var flow = NewFlow();
        flow.AddProcess(process, order: 1);

        var failingHandler = new FakeActivityHandler(_ => throw new InvalidOperationException("boom"));
        var nextHandler = new FakeActivityHandler();
        var resolver = new FakeActivityHandlerResolver()
            .Register("failing", failingHandler)
            .Register("next", nextHandler);
        var executor = new ProcessFlowExecutor(resolver);

        var result = await executor.ExecuteAsync(flow);

        result.ProcessExecutions.Single().ActivityExecutions.Should().HaveCount(2);
        nextHandler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task ProcessFailure_WithContinueDisabled_SkipsRemainingProcesses()
    {
        var failing = NewActivity("failing");
        var failingProcess = NewProcess();
        failingProcess.AddActivity(failing, order: 1);

        var ok = NewActivity("ok");
        var okProcess = NewProcess();
        okProcess.AddActivity(ok, order: 1);

        var flow = NewFlow(continueOnProcessFailure: false);
        flow.AddProcess(failingProcess, order: 1);
        flow.AddProcess(okProcess, order: 2);

        var okHandler = new FakeActivityHandler();
        var resolver = new FakeActivityHandlerResolver()
            .Register("failing", new FakeActivityHandler(_ => throw new InvalidOperationException("boom")))
            .Register("ok", okHandler);
        var executor = new ProcessFlowExecutor(resolver);

        var result = await executor.ExecuteAsync(flow);

        result.Status.Should().Be(ExecutionStatus.Failed);
        result.ProcessExecutions.Should().ContainSingle();
        okHandler.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task RetriesUpToMaxRetryCount_ThenSucceeds()
    {
        var attempts = 0;
        var activity = NewActivity("flaky", maxRetryCount: 2);
        var process = NewProcess();
        process.AddActivity(activity, order: 1);
        var flow = NewFlow();
        flow.AddProcess(process, order: 1);

        var handler = new FakeActivityHandler(_ =>
        {
            attempts++;
            if (attempts < 3)
                throw new InvalidOperationException("transient");
            return Task.FromResult<string?>(null);
        });
        var resolver = new FakeActivityHandlerResolver().Register("flaky", handler);
        var executor = new ProcessFlowExecutor(resolver);

        var result = await executor.ExecuteAsync(flow);

        var activityExecution = result.ProcessExecutions.Single().ActivityExecutions.Single();
        activityExecution.Status.Should().Be(ExecutionStatus.Completed);
        activityExecution.RetryCount.Should().Be(2);
        handler.CallCount.Should().Be(3);
    }

    [Fact]
    public async Task UnregisteredActivityType_FailsWithoutRetrying()
    {
        var activity = NewActivity("unknown", maxRetryCount: 3);
        var process = NewProcess();
        process.AddActivity(activity, order: 1);
        var flow = NewFlow();
        flow.AddProcess(process, order: 1);

        var executor = new ProcessFlowExecutor(new FakeActivityHandlerResolver());

        var result = await executor.ExecuteAsync(flow);

        var activityExecution = result.ProcessExecutions.Single().ActivityExecutions.Single();
        activityExecution.Status.Should().Be(ExecutionStatus.Failed);
        activityExecution.RetryCount.Should().Be(0);
    }
}
