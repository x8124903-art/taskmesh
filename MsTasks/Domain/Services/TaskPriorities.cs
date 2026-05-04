namespace MsTasks.Domain.Services;

public static class TaskPriorities
{
    public static readonly TaskPriority[] All = Enum.GetValues<TaskPriority>();

    public static bool IsValid(TaskPriority priority) => Enum.IsDefined(priority);
}
