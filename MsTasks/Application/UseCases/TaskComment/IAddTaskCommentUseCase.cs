using MsTasks.Application.Models;

namespace MsTasks.Application.UseCases.TaskComment;

public interface IAddTaskCommentUseCase
{
    Task<TaskCommentModel> ExecuteAsync(int taskId, AddTaskCommentRequest request, int currentUserId, CancellationToken cancellationToken = default);
}
