using MsTasks.Domain.Services;

namespace MsTasks.Application.UseCases.Task;

public sealed class DeleteTaskUseCase(ITaskService taskService) : IDeleteTaskUseCase
{
    public async System.Threading.Tasks.Task ExecuteAsync(int taskId, int currentUserId, CancellationToken cancellationToken = default)
    {
        await taskService.DeleteAsync(taskId, currentUserId, cancellationToken);
    }
}
