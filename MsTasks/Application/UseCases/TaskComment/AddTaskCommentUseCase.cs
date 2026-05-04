using MsTasks.Application.Models;
using MsTasks.Domain.Services;

namespace MsTasks.Application.UseCases.TaskComment;

public sealed class AddTaskCommentUseCase(ITaskCommentService taskCommentService) : IAddTaskCommentUseCase
{
    public async Task<TaskCommentModel> ExecuteAsync(int taskId, AddTaskCommentRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        return await taskCommentService.AddCommentAsync(taskId, request, currentUserId, cancellationToken);
    }
}
