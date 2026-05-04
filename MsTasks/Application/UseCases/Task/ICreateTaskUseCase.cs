using MsTasks.Application.Models;

namespace MsTasks.Application.UseCases.Task;

public interface ICreateTaskUseCase
{
    Task<TaskModel> ExecuteAsync(AddTaskRequest request, int currentUserId, CancellationToken cancellationToken = default);
}
