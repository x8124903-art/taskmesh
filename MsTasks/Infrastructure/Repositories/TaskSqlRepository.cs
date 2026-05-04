using MsTasks.Application.Models;
using MsTasks.Domain;
using MsTasks.Infrastructure.Data;
using MsTasks.Infrastructure.Persistence;

namespace MsTasks.Infrastructure.Repositories;

public sealed class TaskSqlRepository : ITaskRepository
{
    private readonly IDapperContext _context;

    public TaskSqlRepository(IDapperContext context)
    {
        _context = context;
    }

    public async Task<TaskModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var entity = await _context.QueryFirstOrDefaultAsync<TaskEntity>(
            connection,
            QueriesMySql.GetById,
            new { IdTask = id },
            cancellationToken);
        
        return entity is not null ? ToModel(entity) : null;
    }

    public async Task<IEnumerable<TaskModel>> GetByProjectIdAsync(
        int projectId,
        TaskStatus? status,
        TaskPriority? priority,
        int? assignedToUserId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var entities = await _context.QueryAsync<TaskEntity>(
            connection,
            QueriesMySql.GetByProjectId,
            new { ProjectId = projectId, Status = (int?)status, Priority = (int?)priority, AssignedToUserId = assignedToUserId },
            cancellationToken);
        
        return entities.Select(ToModel);
    }

    public async Task<TaskModel> CreateAsync(TaskModel task, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var id = await _context.ExecuteScalarAsync<int>(
            connection,
            QueriesMySql.Create,
            new
            {
                task.Title,
                task.Description,
                task.ProjectId,
                task.AssignedToUserId,
                Priority = (int)task.Priority,
                Status = (int)task.Status,
                task.DueDate,
                task.CreatedBy
            },
            cancellationToken);
        
        return task with { IdTask = id, RowVersion = 1 };
    }

    public async Task<int> UpdateAsync(TaskModel task, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        return await _context.ExecuteAsync(
            connection,
            QueriesMySql.Update,
            new
            {
                task.IdTask,
                task.Title,
                task.Description,
                task.AssignedToUserId,
                Priority = (int)task.Priority,
                task.DueDate,
                task.RowVersion
            },
            cancellationToken);
    }

    public async Task<int> UpdateStatusAsync(int id, TaskStatus status, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        return await _context.ExecuteAsync(
            connection,
            QueriesMySql.UpdateStatus,
            new { IdTask = id, Status = (int)status },
            cancellationToken);
    }

    public async Task<int> UpdateAssignmentAsync(int id, int? assignedToUserId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        return await _context.ExecuteAsync(
            connection,
            QueriesMySql.UpdateAssignment,
            new { IdTask = id, AssignedToUserId = assignedToUserId },
            cancellationToken);
    }

    public async Task<int> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        return await _context.ExecuteAsync(
            connection,
            QueriesMySql.SoftDelete,
            new { IdTask = id },
            cancellationToken);
    }

    public async Task<IEnumerable<TaskModel>> GetBoardByProjectIdAsync(int projectId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var entities = await _context.QueryAsync<TaskEntity>(
            connection,
            QueriesMySql.GetBoardByProjectId,
            new { ProjectId = projectId },
            cancellationToken);
        
        return entities.Select(ToModel);
    }

    private static TaskModel ToModel(TaskEntity entity) => new(
        entity.IdTask,
        entity.Title,
        entity.Description,
        entity.ProjectId,
        entity.AssignedToUserId,
        (TaskPriority)entity.Priority,
        (TaskStatus)entity.Status,
        entity.DueDate,
        entity.CreatedBy,
        entity.RowVersion,
        entity.CreatedAt,
        entity.UpdatedAt
    );
}
