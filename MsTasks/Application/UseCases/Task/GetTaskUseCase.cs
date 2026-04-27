using MsTasks.Application.Models;
using MsTasks.Domain.Services;

namespace MsTasks.Application.UseCases.Task;

public sealed class GetTaskUseCase(ITaskService taskService) : IGetTaskUseCase
{
    public async Task<TaskModel?> ExecuteAsync(int taskId, int currentUserId, CancellationToken cancellationToken = default)
    {
        return await taskService.GetByIdAsync(taskId, currentUserId, cancellationToken);
    }
}
