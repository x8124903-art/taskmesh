using MsTasks.Application.Models;

namespace MsTasks.Application.UseCases.Task;

public interface IGetTaskUseCase
{
    Task<TaskModel?> ExecuteAsync(int taskId, int currentUserId, CancellationToken cancellationToken = default);
}
