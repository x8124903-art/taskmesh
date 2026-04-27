using MsTasks.Application.Models;

namespace MsTasks.Domain.Services;

public interface ITaskService
{
    Task<TaskModel> CreateAsync(AddTaskRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<TaskModel?> GetByIdAsync(int taskId, int currentUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskModel>> GetByProjectIdAsync(int projectId, int currentUserId, TaskStatus? status = null, TaskPriority? priority = null, int? assignedToUserId = null, CancellationToken cancellationToken = default);
    Task<TaskModel> UpdateAsync(int taskId, UpdateTaskRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int taskId, int currentUserId, CancellationToken cancellationToken = default);
    Task<TaskModel> AssignAsync(int taskId, AssignTaskRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<TaskModel> ChangeStatusAsync(int taskId, ChangeStatusRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<BoardResponse> GetBoardAsync(int projectId, int currentUserId, CancellationToken cancellationToken = default);
}
