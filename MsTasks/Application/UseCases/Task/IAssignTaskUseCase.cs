using MsTasks.Application.Models;

namespace MsTasks.Application.UseCases.Task;

public interface IAssignTaskUseCase
{
    Task<TaskModel> ExecuteAsync(int taskId, AssignTaskRequest request, int currentUserId, CancellationToken cancellationToken = default);
}
