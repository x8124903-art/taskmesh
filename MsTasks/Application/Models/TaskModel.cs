using MsTasks.Domain;

namespace MsTasks.Application.Models;

public sealed record TaskModel(
    int IdTask,
    string Title,
    string? Description,
    int ProjectId,
    int? AssignedToUserId,
    TaskPriority Priority,
    TaskStatus Status,
    DateTime? DueDate,
    int CreatedBy,
    int RowVersion,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
