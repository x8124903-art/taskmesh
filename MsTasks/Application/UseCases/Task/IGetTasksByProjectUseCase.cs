using MsTasks.Application.Models;
using MsTasks.Domain;

namespace MsTasks.Application.UseCases.Task;

public interface IGetTasksByProjectUseCase
{
    Task<IEnumerable<TaskModel>> ExecuteAsync(int projectId, int currentUserId, TaskStatus? status = null, TaskPriority? priority = null, int? assignedToUserId = null, CancellationToken cancellationToken = default);
}
