using MsTasks.Application.Models;
using MsTasks.Domain.Services;

namespace MsTasks.Application.UseCases.Task;

public sealed class ChangeTaskStatusUseCase(ITaskService taskService) : IChangeTaskStatusUseCase
{
    public async Task<TaskModel> ExecuteAsync(int taskId, ChangeStatusRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        return await taskService.ChangeStatusAsync(taskId, request, currentUserId, cancellationToken);
    }
}
