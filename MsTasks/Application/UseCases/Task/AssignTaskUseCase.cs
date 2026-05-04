using MsTasks.Application.Models;
using MsTasks.Domain.Services;

namespace MsTasks.Application.UseCases.Task;

public sealed class AssignTaskUseCase(ITaskService taskService) : IAssignTaskUseCase
{
    public async Task<TaskModel> ExecuteAsync(int taskId, AssignTaskRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        return await taskService.AssignAsync(taskId, request, currentUserId, cancellationToken);
    }
}
