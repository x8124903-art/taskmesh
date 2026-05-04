using MsTasks.Application.Models;
using MsTasks.Domain.Services;

namespace MsTasks.Application.UseCases.Task;

public sealed class CreateTaskUseCase(ITaskService taskService) : ICreateTaskUseCase
{
    public async Task<TaskModel> ExecuteAsync(AddTaskRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        return await taskService.CreateAsync(request, currentUserId, cancellationToken);
    }
}
