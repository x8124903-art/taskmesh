using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
internal sealed record TaskCommentEntity(
    int IdTaskComment,
    int TaskId,
    int UserId,
    string Comment,
    DateTime CreatedAt
);
