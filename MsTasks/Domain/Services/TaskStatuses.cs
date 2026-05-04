namespace MsTasks.Domain.Services;

public static class TaskStatuses
{
    public static readonly TaskStatus[] All = Enum.GetValues<TaskStatus>();

    public static bool IsValid(TaskStatus status) => Enum.IsDefined(status);
}
