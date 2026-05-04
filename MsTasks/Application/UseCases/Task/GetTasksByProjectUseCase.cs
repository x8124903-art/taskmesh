using MsTasks.Application.Models;
using MsTasks.Domain;
using MsTasks.Domain.Services;

namespace MsTasks.Application.UseCases.Task;

public sealed class GetTasksByProjectUseCase(ITaskService taskService) : IGetTasksByProjectUseCase
{
    public async Task<IEnumerable<TaskModel>> ExecuteAsync(int projectId, int currentUserId, TaskStatus? status = null, TaskPriority? priority = null, int? assignedToUserId = null, CancellationToken cancellationToken = default)
    {
        return await taskService.GetByProjectIdAsync(projectId, currentUserId, status, priority, assignedToUserId, cancellationToken);
    }
}
