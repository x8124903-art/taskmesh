using MsTasks.Application.Models;

namespace MsTasks.Application.UseCases.TaskComment;

public interface IGetTaskCommentsUseCase
{
    Task<IEnumerable<TaskCommentModel>> ExecuteAsync(int taskId, int currentUserId, CancellationToken cancellationToken = default);
}
