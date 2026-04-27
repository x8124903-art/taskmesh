using MsTasks.Domain;

namespace MsTasks.Application.Models;

public sealed record AddTaskRequest(
    string Title,
    string? Description,
    int ProjectId,
    int? AssignedToUserId,
    TaskPriority? Priority,
    DateTime? DueDate
);
