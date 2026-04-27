namespace MsTasks.Domain.Exceptions;

public sealed class TaskNotFoundException : DomainException
{
    public TaskNotFoundException(int taskId)
        : base($"Task with ID {taskId} not found.")
    {
    }
}
