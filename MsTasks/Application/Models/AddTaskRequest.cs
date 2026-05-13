using MsTasks.Domain;

using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Application.Models;

[ExcludeFromCodeCoverage]
public sealed record AddTaskRequest(
    string Title,
    string? Description,
    int ProjectId,
    int? AssignedToUserId,
    TaskPriority? Priority,
    DateTime? DueDate
);
