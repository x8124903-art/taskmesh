using MsTasks.Application.Models;

namespace MsTasks.Domain.Services;

public interface ITaskCommentService
{
    Task<TaskCommentModel> AddCommentAsync(int taskId, AddTaskCommentRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskCommentModel>> GetCommentsAsync(int taskId, int currentUserId, CancellationToken cancellationToken = default);
}
