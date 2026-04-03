namespace MsProjects.Domain.Services;

/// <summary>
/// Project status IDs as stored in ProjectStatus table
/// </summary>
public static class ProjectStatuses
{
    public const int Active = 1;
    public const int Paused = 2;
    public const int Completed = 3;
    public const int Archived = 4;

    public static string GetName(int id) => id switch
    {
        Active => "Active",
        Paused => "Paused",
        Completed => "Completed",
        Archived => "Archived",
        _ => "Unknown"
    };
}
