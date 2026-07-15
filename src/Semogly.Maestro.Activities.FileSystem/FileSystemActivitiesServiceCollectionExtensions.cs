using Microsoft.Extensions.DependencyInjection;
using Semogly.Maestro.Abstractions.Activities;

namespace Semogly.Maestro.Activities.FileSystem;

public static class FileSystemActivitiesServiceCollectionExtensions
{
    /// <summary>Registers MoveFileActivityHandler, CopyFileActivityHandler and DeleteFileActivityHandler under their FileSystemActivityTypes keys.</summary>
    public static IServiceCollection AddFileSystemActivities(this IServiceCollection services) =>
        services
            .AddActivityHandler<MoveFileActivityHandler>(FileSystemActivityTypes.MoveFile)
            .AddActivityHandler<CopyFileActivityHandler>(FileSystemActivityTypes.CopyFile)
            .AddActivityHandler<DeleteFileActivityHandler>(FileSystemActivityTypes.DeleteFile);
}
