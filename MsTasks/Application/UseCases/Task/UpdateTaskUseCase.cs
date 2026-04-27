using MsTasks.Application.Models;
using MsTasks.Domain.Services;

namespace MsTasks.Application.UseCases.Task;

public sealed class UpdateTaskUseCase(ITaskService taskService) : IUpdateTaskUseCase
{
    public async Task<TaskModel> ExecuteAsync(int taskId, UpdateTaskRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        return await taskService.UpdateAsync(taskId, request, currentUserId, cancellationToken);
    }
}
