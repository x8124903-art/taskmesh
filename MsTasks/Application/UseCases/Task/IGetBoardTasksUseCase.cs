using MsTasks.Application.Models;

namespace MsTasks.Application.UseCases.Task;

public interface IGetBoardTasksUseCase
{
    Task<BoardResponse> ExecuteAsync(int projectId, int currentUserId, CancellationToken cancellationToken = default);
}
