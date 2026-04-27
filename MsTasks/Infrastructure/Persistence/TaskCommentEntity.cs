namespace MsTasks.Infrastructure.Persistence;

internal sealed record TaskCommentEntity(
    int IdTaskComment,
    int TaskId,
    int UserId,
    string Comment,
    DateTime CreatedAt
);
