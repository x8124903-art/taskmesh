using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Domain.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class TaskNotFoundException : DomainException
{
    public TaskNotFoundException(int taskId)
        : base($"Task with ID {taskId} not found.")
    {
    }
}
