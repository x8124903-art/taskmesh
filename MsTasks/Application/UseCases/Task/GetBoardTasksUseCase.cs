using MsTasks.Application.Models;
using MsTasks.Domain.Services;

namespace MsTasks.Application.UseCases.Task;

public sealed class GetBoardTasksUseCase(ITaskService taskService) : IGetBoardTasksUseCase
{
    public async Task<BoardResponse> ExecuteAsync(int projectId, int currentUserId, CancellationToken cancellationToken = default)
    {
        return await taskService.GetBoardAsync(projectId, currentUserId, cancellationToken);
    }
}
