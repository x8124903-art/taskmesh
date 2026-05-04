using MsTasks.Application.Models;

namespace MsTasks.Infrastructure.Repositories;

public interface ITaskCommentRepository
{
    Task<TaskCommentModel> CreateAsync(int taskId, int userId, string comment, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskCommentModel>> GetByTaskIdAsync(int taskId, CancellationToken cancellationToken = default);
}
