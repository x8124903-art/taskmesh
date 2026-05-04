using MsTasks.Application.Models;
using MsTasks.Domain.Services;

namespace MsTasks.Application.UseCases.TaskComment;

public sealed class GetTaskCommentsUseCase(ITaskCommentService taskCommentService) : IGetTaskCommentsUseCase
{
    public async Task<IEnumerable<TaskCommentModel>> ExecuteAsync(int taskId, int currentUserId, CancellationToken cancellationToken = default)
    {
        return await taskCommentService.GetCommentsAsync(taskId, currentUserId, cancellationToken);
    }
}
