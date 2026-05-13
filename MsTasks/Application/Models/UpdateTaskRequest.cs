using MsTasks.Domain;

using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Application.Models;

[ExcludeFromCodeCoverage]
public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    int? AssignedToUserId,
    TaskPriority Priority,
    DateTime? DueDate,
    int RowVersion
);
