using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Activities.FileSystem;

/// <summary>Factory methods for the Activity/ActivityParam definitions matching each handler in this package — keeps the param keys a handler reads in sync with the ones seeded for it.</summary>
public static class FileSystemActivityDefinitions
{
    public static Activity MoveFile()
    {
        var activity = new Activity { Name = "Move File", Type = FileSystemActivityTypes.MoveFile, Active = true };
        AddParam(activity, "SourcePath", required: true);
        AddParam(activity, "DestinationPath", required: true);
        AddParam(activity, "OverwriteExisting", required: false, defaultValue: "false");
        return activity;
    }

    public static Activity CopyFile()
    {
        var activity = new Activity { Name = "Copy File", Type = FileSystemActivityTypes.CopyFile, Active = true };
        AddParam(activity, "SourcePath", required: true);
        AddParam(activity, "DestinationPath", required: true);
        AddParam(activity, "OverwriteExisting", required: false, defaultValue: "false");
        return activity;
    }

    public static Activity DeleteFile()
    {
        var activity = new Activity { Name = "Delete File", Type = FileSystemActivityTypes.DeleteFile, Active = true };
        AddParam(activity, "Path", required: true);
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
