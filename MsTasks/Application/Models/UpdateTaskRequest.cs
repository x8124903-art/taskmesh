using MsTasks.Domain;

namespace MsTasks.Application.Models;

public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    int? AssignedToUserId,
    TaskPriority Priority,
    DateTime? DueDate,
    int RowVersion
);
