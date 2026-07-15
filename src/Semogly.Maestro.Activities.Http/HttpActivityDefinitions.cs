using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Activities.Http;

/// <summary>Factory methods for the Activity/ActivityParam definitions matching each handler in this package — keeps the param keys a handler reads in sync with the ones seeded for it.</summary>
public static class HttpActivityDefinitions
{
    public static Activity Request()
    {
        var activity = new Activity { Name = "HTTP Request", Type = HttpActivityTypes.Request, Active = true };
        AddParam(activity, "Url", required: true);
        AddParam(activity, "Method", required: false, defaultValue: "GET");
        AddParam(activity, "Body", required: false);
        AddParam(activity, "ContentType", required: false, defaultValue: "application/json");
        AddParam(activity, "Headers", required: false);
        return activity;
    }

    private static void AddParam(Activity activity, string key, bool required, string? defaultValue = null) =>
        activity.Params.Add(new ActivityParam
        {
            IdActivity = activity.Id,
            Activity = activity,
            Key = key,
            Required = required,
            DefaultValue = defaultValue,
        });
}
