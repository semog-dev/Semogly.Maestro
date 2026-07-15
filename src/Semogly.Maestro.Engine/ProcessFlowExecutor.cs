using Semogly.Maestro.Abstractions.Activities;
using Semogly.Maestro.Core.Entities;
using Semogly.Maestro.Core.Enums;

namespace Semogly.Maestro.Engine;

public sealed class ProcessFlowExecutor(IActivityHandlerResolver handlerResolver, TimeProvider? timeProvider = null)
    : IProcessFlowExecutor
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<ProcessFlowExecution> ExecuteAsync(ProcessFlow processFlow, CancellationToken cancellationToken = default)
    {
        var flowExecution = new ProcessFlowExecution
        {
            IdProcessFlow = processFlow.Id,
            Status = ExecutionStatus.Running,
            StartedAt = _timeProvider.GetUtcNow(),
        };

        var flowFailed = false;

        foreach (var link in processFlow.Processes.OrderBy(x => x.Order))
        {
            if (flowFailed && !processFlow.ContinueOnProcessFailure)
                break;

            var processExecution = await ExecuteProcessAsync(link, flowExecution.Id, cancellationToken);
            flowExecution.ProcessExecutions.Add(processExecution);

            if (processExecution.Status == ExecutionStatus.Failed)
                flowFailed = true;
        }

        flowExecution.Status = flowFailed ? ExecutionStatus.Failed : ExecutionStatus.Completed;
        flowExecution.FinishedAt = _timeProvider.GetUtcNow();

        return flowExecution;
    }

    private async Task<ProcessExecution> ExecuteProcessAsync(
        ProcessFlowProcess link, Guid flowExecutionId, CancellationToken cancellationToken)
    {
        var processExecution = new ProcessExecution
        {
            IdProcessFlowExecution = flowExecutionId,
            IdProcessFlowProcess = link.Id,
            Status = ExecutionStatus.Running,
            StartedAt = _timeProvider.GetUtcNow(),
        };

        var processFailed = false;

        foreach (var processActivity in link.Process.Activities.OrderBy(x => x.Order))
        {
            if (processFailed && !link.Process.ContinueOnActivityFailure)
                break;

            var activityExecution = await ExecuteActivityAsync(processActivity, processExecution.Id, cancellationToken);
            processExecution.ActivityExecutions.Add(activityExecution);

            if (activityExecution.Status == ExecutionStatus.Failed)
                processFailed = true;
        }

        processExecution.Status = processFailed ? ExecutionStatus.Failed : ExecutionStatus.Completed;
        processExecution.FinishedAt = _timeProvider.GetUtcNow();

        return processExecution;
    }

    private async Task<ActivityExecution> ExecuteActivityAsync(
        ProcessActivity processActivity, Guid processExecutionId, CancellationToken cancellationToken)
    {
        var activityExecution = new ActivityExecution
        {
            IdProcessExecution = processExecutionId,
            IdProcessActivity = processActivity.Id,
            Status = ExecutionStatus.Running,
            StartedAt = _timeProvider.GetUtcNow(),
        };

        if (!TryResolveParameters(processActivity, out var parameters, out var validationError))
        {
            activityExecution.Status = ExecutionStatus.Failed;
            activityExecution.Error = validationError;
            activityExecution.FinishedAt = _timeProvider.GetUtcNow();
            return activityExecution;
        }

        var context = new ActivityExecutionContext
        {
            ActivityExecutionId = activityExecution.Id,
            ProcessActivityId = processActivity.Id,
            Parameters = parameters,
        };

        var maxAttempts = processActivity.Activity.MaxRetryCount + 1;
        Exception? lastError = null;
        string? output = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            activityExecution.RetryCount = attempt - 1;

            try
            {
                var handler = handlerResolver.Resolve(processActivity.Activity.Type);
                output = await handler.ExecuteAsync(context, cancellationToken);
                lastError = null;
                break;
            }
            catch (ActivityHandlerNotFoundException ex)
            {
                // Retrying won't register a handler that doesn't exist.
                lastError = ex;
                break;
            }
            catch (Exception ex)
            {
                lastError = ex;
            }
        }

        activityExecution.Status = lastError is null ? ExecutionStatus.Completed : ExecutionStatus.Failed;
        activityExecution.Error = lastError?.Message;
        activityExecution.Output = lastError is null ? output : null;
        activityExecution.FinishedAt = _timeProvider.GetUtcNow();

        return activityExecution;
    }

    private static bool TryResolveParameters(
        ProcessActivity processActivity, out Dictionary<string, string?> parameters, out string? error)
    {
        parameters = new Dictionary<string, string?>();

        foreach (var definition in processActivity.Activity.Params)
        {
            var value = processActivity.Params.FirstOrDefault(x => x.Key == definition.Key)?.Value
                ?? definition.DefaultValue;

            if (definition.Required && value is null)
            {
                error = $"Required parameter '{definition.Key}' has no value.";
                return false;
            }

            parameters[definition.Key] = value;
        }

        error = null;
        return true;
    }
}
