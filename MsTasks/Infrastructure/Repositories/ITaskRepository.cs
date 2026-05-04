using MsTasks.Application.Models;
using MsTasks.Domain;

namespace MsTasks.Infrastructure.Repositories;

public interface ITaskRepository
{
    Task<TaskModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskModel>> GetByProjectIdAsync(int projectId, TaskStatus? status, TaskPriority? priority, int? assignedToUserId, CancellationToken cancellationToken = default);
    Task<TaskModel> CreateAsync(TaskModel task, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(TaskModel task, CancellationToken cancellationToken = default);
    Task<int> UpdateStatusAsync(int id, TaskStatus status, CancellationToken cancellationToken = default);
    Task<int> UpdateAssignmentAsync(int id, int? assignedToUserId, CancellationToken cancellationToken = default);
    Task<int> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskModel>> GetBoardByProjectIdAsync(int projectId, CancellationToken cancellationToken = default);
}
