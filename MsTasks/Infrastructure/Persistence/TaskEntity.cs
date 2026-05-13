using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
internal sealed record TaskEntity(
    int IdTask,
    string Title,
    string? Description,
    int ProjectId,
    int? AssignedToUserId,
    int Priority,
    int Status,
    DateTime? DueDate,
    int CreatedBy,
    int RowVersion,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsDeleted,
    DateTime? DeletedAt
);
