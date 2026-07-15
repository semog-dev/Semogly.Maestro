using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Engine.Tests;

internal static class GraphBuilder
{
    public static ProcessFlow NewFlow(bool continueOnProcessFailure = false) =>
        new() { Name = "flow", ContinueOnProcessFailure = continueOnProcessFailure };

    public static Process NewProcess(bool continueOnActivityFailure = false) =>
        new() { Name = "process", ContinueOnActivityFailure = continueOnActivityFailure };

    public static Activity NewActivity(string type, int maxRetryCount = 0) =>
        new() { Name = type, Type = type, MaxRetryCount = maxRetryCount };

    public static ActivityParam AddParam(this Activity activity, string key, bool required, string? defaultValue = null)
    {
        var param = new ActivityParam { IdActivity = activity.Id, Key = key, Required = required, DefaultValue = defaultValue };
        activity.Params.Add(param);
        return param;
    }

    public static ProcessFlowProcess AddProcess(this ProcessFlow flow, Process process, int order)
    {
        var link = new ProcessFlowProcess { IdProcessFlow = flow.Id, IdProcess = process.Id, Order = order, ProcessFlow = flow, Process = process };
        flow.Processes.Add(link);
        return link;
    }

    public static ProcessActivity AddActivity(this Process process, Activity activity, int order)
    {
        var link = new ProcessActivity { IdProcess = process.Id, IdActivity = activity.Id, Order = order, Process = process, Activity = activity };
        process.Activities.Add(link);
        return link;
    }

    public static ProcessActivityParam SetParam(this ProcessActivity processActivity, string key, string? value)
    {
        var param = new ProcessActivityParam { IdProcessActivity = processActivity.Id, Key = key, Value = value };
        processActivity.Params.Add(param);
        return param;
    }
}
