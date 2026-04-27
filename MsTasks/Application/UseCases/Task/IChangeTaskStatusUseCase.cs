using MsTasks.Application.Models;

namespace MsTasks.Application.UseCases.Task;

public interface IChangeTaskStatusUseCase
{
    Task<TaskModel> ExecuteAsync(int taskId, ChangeStatusRequest request, int currentUserId, CancellationToken cancellationToken = default);
}
