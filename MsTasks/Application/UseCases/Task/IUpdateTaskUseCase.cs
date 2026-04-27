using MsTasks.Application.Models;

namespace MsTasks.Application.UseCases.Task;

public interface IUpdateTaskUseCase
{
    Task<TaskModel> ExecuteAsync(int taskId, UpdateTaskRequest request, int currentUserId, CancellationToken cancellationToken = default);
}
