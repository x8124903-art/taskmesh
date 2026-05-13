using MsTasks.Domain;
using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Application.Models;

[ExcludeFromCodeCoverage]
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
