using MsTasks.Application.Models;
using MsTasks.Infrastructure.Data;
using MsTasks.Infrastructure.Persistence;

namespace MsTasks.Infrastructure.Repositories;

public sealed class TaskCommentSqlRepository : ITaskCommentRepository
{
    private readonly IDapperContext _context;

    public TaskCommentSqlRepository(IDapperContext context)
    {
        _context = context;
    }

    public async Task<TaskCommentModel> CreateAsync(int taskId, int userId, string comment, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var id = await _context.ExecuteScalarAsync<int>(
            connection,
            QueriesMySql.CreateComment,
            new { TaskId = taskId, UserId = userId, Comment = comment },
            cancellationToken);
        
        return new TaskCommentModel(id, taskId, userId, comment, DateTime.UtcNow);
    }

    public async Task<IEnumerable<TaskCommentModel>> GetByTaskIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var entities = await _context.QueryAsync<TaskCommentEntity>(
            connection,
            QueriesMySql.GetCommentsByTaskId,
            new { TaskId = taskId },
            cancellationToken);
        
        return entities.Select(ToModel);
    }

    private static TaskCommentModel ToModel(TaskCommentEntity entity) => new(
        entity.IdTaskComment,
        entity.TaskId,
        entity.UserId,
        entity.Comment,
        entity.CreatedAt
    );
}
