namespace Semogly.Maestro.Activities.FileSystem;

/// <summary>Activity.Type values handled by this package — register handlers under these keys in your IActivityHandlerResolver.</summary>
public static class FileSystemActivityTypes
{
    public const string MoveFile = "file-move";
    public const string CopyFile = "file-copy";
    public const string DeleteFile = "file-delete";
}
